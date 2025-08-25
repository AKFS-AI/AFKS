using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace AFKS.EditorTools
{
    /// <summary>
    /// 체인 UI(예: ChainRoot)에 사용할 AnimatorController와 애니메이션 클립(Shake/Break)을 생성하고 할당하는 유틸리티입니다.
    /// 사용법: Hierarchy에서 체인 루트를 선택한 뒤, 메뉴 AFKS/Animation/Create Chain Animator 실행.
    /// </summary>
    public static class ChainAnimationCreator
    {
        private const string MenuPath = "AFKS/Animation/Create Chain Animator (Assign to Selected)";

        [MenuItem(MenuPath)]
        private static void CreateAndAssign()
        {
            var go = Selection.activeGameObject;
            if (go == null)
            {
                EditorUtility.DisplayDialog("Chain Animator", "체인 루트 GameObject를 선택한 후 다시 실행하세요.", "OK");
                return;
            }

            var rect = go.GetComponent<RectTransform>();
            if (rect == null)
            {
                EditorUtility.DisplayDialog("Chain Animator", "선택한 오브젝트에 RectTransform이 없습니다. UI 체인 루트를 선택하세요.", "OK");
                return;
            }

            var animator = go.GetComponent<Animator>();
            if (animator == null) animator = go.AddComponent<Animator>();

            // 에셋 폴더 준비
            var rootFolder = "Assets/Animations";
            if (!AssetDatabase.IsValidFolder(rootFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Animations");
            }

            // 파일 경로 결정
            var safeName = go.name.Replace(' ', '_');
            var ctrlPath = $"{rootFolder}/{safeName}_Chain.controller";
            var shakeClipPath = $"{rootFolder}/{safeName}_Chain_Shake.anim";
            var breakClipPath = $"{rootFolder}/{safeName}_Chain_Break.anim";

            // 클립 생성
            var shakeClip = CreateShakeClip();
            AssetDatabase.CreateAsset(shakeClip, GetUniquePath(shakeClipPath));

            var breakClip = CreateBreakClip();
            // 종료 시 컨트롤러의 메서드를 호출하는 애니메이션 이벤트 추가(선택)
            var evt = new AnimationEvent { time = breakClip.length - 0.01f, functionName = "OnBreakAnimationCompleted" };
            AnimationUtility.SetAnimationEvents(breakClip, new[] { evt });
            AssetDatabase.CreateAsset(breakClip, GetUniquePath(breakClipPath));

            // 컨트롤러 생성 및 상태 구성
            var controller = AnimatorController.CreateAnimatorControllerAtPath(GetUniquePath(ctrlPath));
            controller.AddParameter("Shake", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Break", AnimatorControllerParameterType.Trigger);

            var rootStateMachine = controller.layers[0].stateMachine;
            var idleState = rootStateMachine.AddState("Idle");
            rootStateMachine.defaultState = idleState;

            var shakeState = rootStateMachine.AddState("Shake");
            shakeState.motion = shakeClip;
            shakeState.writeDefaultValues = true;

            var breakState = rootStateMachine.AddState("Break");
            breakState.motion = breakClip;
            breakState.writeDefaultValues = true;

            // AnyState 전이 설정
            var toShake = rootStateMachine.AddAnyStateTransition(shakeState);
            toShake.hasExitTime = false;
            toShake.duration = 0.05f;
            toShake.AddCondition(AnimatorConditionMode.If, 0, "Shake");

            var toBreak = rootStateMachine.AddAnyStateTransition(breakState);
            toBreak.hasExitTime = false;
            toBreak.duration = 0.05f;
            toBreak.AddCondition(AnimatorConditionMode.If, 0, "Break");

            // Shake 종료 후 Idle로 복귀
            var backToIdle = shakeState.AddTransition(idleState);
            backToIdle.hasExitTime = true;
            backToIdle.exitTime = 0.99f;
            backToIdle.duration = 0.05f;

            // Break는 종료 상태(다음 전이는 스크립트에서 처리: 파괴 또는 루트 비활성)

            // 할당 및 저장
            animator.runtimeAnimatorController = controller;
            EditorUtility.SetDirty(animator);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Chain Animator", "체인 애니메이션(Shake/Break) 컨트롤러가 생성되어 선택 오브젝트에 할당되었습니다.", "OK");
        }

        /// <summary>
        /// 좌우 흔들림(anchoredPosition.x)을 0.3초 동안 왕복시켜 흔들리는 효과를 만듭니다.
        /// </summary>
        private static AnimationClip CreateShakeClip()
        {
            var clip = new AnimationClip
            {
                name = "Chain_Shake",
                frameRate = 60f
            };

            // 키프레임: 0 -> +6 -> -6 -> +6 -> -6 -> +3 -> 0 (0.30s)
            var times = new[] { 0f, 0.05f, 0.10f, 0.15f, 0.20f, 0.25f, 0.30f };
            var values = new[] { 0f, 6f, -6f, 6f, -6f, 3f, 0f };

            var curveX = new AnimationCurve();
            for (int i = 0; i < times.Length; i++)
            {
                var kf = new Keyframe(times[i], values[i]) { inTangent = 0f, outTangent = 0f };
                curveX.AddKey(kf);
            }

            // Y는 0 고정(현재 값 기준 상대가 아니라 절대값이므로 0으로 유지)
            var curveY = AnimationCurve.Constant(0f, 0.30f, 0f);

            // RectTransform의 앵커드 포지션 경로
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(RectTransform), "m_AnchoredPosition.x"), curveX);
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(RectTransform), "m_AnchoredPosition.y"), curveY);

            clip.wrapMode = WrapMode.Default;
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = false;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            return clip;
        }

        /// <summary>
        /// 0.8초 동안 스케일을 1 -> 0 으로 줄이며 파괴 직전 연출을 만듭니다.
        /// </summary>
        private static AnimationClip CreateBreakClip()
        {
            var clip = new AnimationClip
            {
                name = "Chain_Break",
                frameRate = 60f
            };

            var time = new[] { 0f, 0.8f };
            var one = new[] { 1f, 0f };

            var scaleX = new AnimationCurve(new Keyframe(time[0], one[0]), new Keyframe(time[1], one[1]));
            var scaleY = new AnimationCurve(new Keyframe(time[0], one[0]), new Keyframe(time[1], one[1]));
            var scaleZ = new AnimationCurve(new Keyframe(time[0], 1f), new Keyframe(time[1], 1f));

            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalScale.x"), scaleX);
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalScale.y"), scaleY);
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalScale.z"), scaleZ);

            clip.wrapMode = WrapMode.Default;
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = false;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            return clip;
        }

        private static string GetUniquePath(string path)
        {
            return AssetDatabase.GenerateUniqueAssetPath(path);
        }
    }
}



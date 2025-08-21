namespace AFKS.Core.Services
{
	public interface ISaveService
	{
		bool TryLoadAll();
		void SaveAll();

		string CurrentStageId { get; set; }

		float MasterVolume { get; set; }
		float BgmVolume { get; set; }
		float SfxVolume { get; set; }
		bool DynamicRange { get; set; }

		bool ReduceFlicker { get; set; }
		bool ReduceShake { get; set; }
	}
}

using UnityEngine;
using System.Collections;

public class FishGeneratConfig : MonoBehaviour {
    //±‰¡ø
    public FishGenerator.FishGenerateData[] FishGenerateDatas;
    public FishGenerator.FishGenerateData[] FishGenerateUniqueDatas;
    public FishGenerator.FishQueueGenerateData[] FishQueueData;

    public FishGenerator.FloatRnd Interval_FishGenerate;
    public FishGenerator.FloatRnd Interval_FishUniqueGenerate;
    public FishGenerator.FloatRnd Interval_QueuGenerate;
    public FishGenerator.FloatRnd Interval_FlockGenerate;



}

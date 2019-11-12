using System;

namespace wazServer
{
    /// <summary>
    /// 请求userid
    /// </summary>
    [Packable(11)]
    public struct CS_RequestUserID
    {
        public string MachineID;
        public string Resolution;//分辨率
    }

    /// <summary>
    /// 分配userid
    /// </summary>
    [Packable(12)]
    public struct SC_AllocUserID
    {
        public uint UserID;
    }

    /// <summary>
    /// 游戏玩家_登入
    /// </summary>
    [Packable(13)]
    public struct CS_GamePlayerLogin
    {
        public uint UserID;
    }

    /// <summary>
    /// 游戏玩家_登出
    /// </summary>
    [Packable(14)]
    public struct CS_GamePlayerLogout
    {
    }

    /// <summary>
    /// 请求排名信息
    /// </summary>
    [Packable(15)]
    public struct CS_RequestRank
    {
    }

    /// <summary>
    /// 返回排名信息(底层暂时只支持基本类型)
    /// </summary>
    [Packable(16)]
    public struct SC_ResponRank
    {
        public uint Rank;
        public string Name;

        public string RankNameOffset2;//比你前两名
        public string RankNameOffset1;//比你前一名
        public string RankNameOffsetN1;//比你后一名
        public string RankNameOffsetN2;//比你后两名

        public int RankOffset2;//比你前两名
        public int RankOffset1;//比你前一名
        public int RankOffsetN1;//比你后一名
        public int RankOffsetN2;//比你后两名
    }

    /// <summary>
    /// 提交分数
    /// </summary>
    [Packable(17)]
    public struct CS_CommitScore
    {
        public int score;
    }

    /// <summary>
    /// 每日奖励派发
    /// </summary>
    [Packable(18)]
    public struct SC_RewardDaily
    {
    }
 
    [Packable(19)]
    public struct CS_PayNotify
    {
        public int PayTypeIdx;
    }

    [Packable(20)]
    public struct CS_RequestChangeName
    {
        public string Name;
    }

    [Packable(21)]
    public struct SC_RequestChangeNameResp
    {
        public bool Result;
    }
}


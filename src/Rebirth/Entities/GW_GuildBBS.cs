namespace Rebirth.Entities
{
    public class GW_GuildBBSComment
    {
        public int m_nSN { get; set; }
        public int m_nCharacterID { get; set; }
        public string m_sText { get; set; }
        public long m_Date { get; set; }
    }

    public class GW_GuildBBSEntryList
    {
        public int m_nSN { get; set; }
        public int m_nEntryID { get; set; }
        public int m_nCharacterID { get; set; }
        public string m_sTitle { get; set; }
        public long m_Date { get; set; }
        public int m_nEmoticon { get; set; }
        public int m_nCommentCount { get; set; }
    }

    public class GW_GuildBBSEntryText
    {
        public string m_sText { get; set; }
    }

    public class GW_GuildSkillRecord
    {
        public int nSkillID { get; set; }
        public short nLevel { get; set; }
        public long dateExpire { get; set; }
        public string sBuyCharacterName { get; set; }
    }
}

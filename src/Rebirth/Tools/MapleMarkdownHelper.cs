namespace Rebirth.Tools
{
	public static class MapleMarkdownHelper
	{
		/// <summary>
		/// Creates a string that the maple client will interpret as having the given 
		/// </summary>
		/// <param name="mod"></param>
		/// <param name="sMsg"></param>
		/// <returns></returns>
		public static string Text(MarkdownText mod, string sMsg, bool bBold = false)
			=> $"{(bBold ? $"#{MarkdownText.Bold}" : "")}#{mod}{sMsg}#{MarkdownText.Black}#{MarkdownText.Normal}";

		public static string Add(Markdown mod, int nObjectID)
			=> $"#{mod}{nObjectID}#";

		public static string PlayerName()
			=> $"#h #";

		public static string Selection(int nIdx, string sMsg)
			=> $"#L{nIdx}#{sMsg}#l";

		public static string Selection(int nIdx, Markdown mod, int nObjectID)
			=> Selection(nIdx, $"#{mod}{nObjectID}#");

		public static string Selection(int nIdx, Markdown mod, int nObjectID, Markdown mod2 = Markdown.None, int nObjectID2 = 0)
		{
			if (mod2 != Markdown.None)
			{
				return Selection(nIdx, $"#{mod}{nObjectID}##{mod2}{nObjectID2}#");
			}
			else
			{
				return Selection(nIdx, $"#{mod}{nObjectID}#");
			}
		}
	}

	public enum MarkdownText
	{
		Blue = 'b',
		Purple = 'd',
		Green = 'g',
		Black = 'k',
		Red = 'r',

		Bold = 'e',
		Normal = 'n'
	}

	public enum Markdown
	{
		None = '0', // hmm
		ItemImage = 'i',
		ItemName = 't',
		MapName = 'm',
		WZImage = 'F',
		MobName = 'o',
		SkillImage = 's',
		SkillName = 'q',
		ProgressBar = 'B',
		NpcName = 'p',
	}
}

namespace Rebirth.Provider.Template.Skill
{
	public sealed class PassiveSkillDataTemplate : AbstractTemplate
	{
		public short nMHPr { get; set; } //v1->nMHPr = 0;
		public short nMMPr { get; set; }//v1->nMMPr = 0;
		public short nCr { get; set; }//v1->nCr = 0;
		public short nCDMin { get; set; }//v1->nCDMin = 0;
		public short nACCr { get; set; }//v1->nACCr = 0;
		public short nEVAr { get; set; }//v1->nEVAr = 0;
		public short nAr { get; set; }//v1->nAr = 0;
		public short nEr { get; set; }//v1->nEr = 0;
		public short nPDDr { get; set; }//v1->nPDDr = 0;
		public short nMDDr { get; set; }//v1->nMDDr = 0;
		public short nPDr { get; set; }//v1->nPDr = 0;
		public short nMDr { get; set; }//v1->nMDr = 0;
		public short nDIPr { get; set; }//v1->nDIPr = 0;
		public short nPDamr { get; set; }//v1->nPDamr = 0;
		public short nMDamr { get; set; }//v1->nMDamr = 0;
		public short nPADr { get; set; }//v1->nPADr = 0;
		public short nMADr { get; set; }//v1->nMADr = 0;
		public short nEXPr { get; set; }//v1->nEXPr = 0;
		public short nIMPr { get; set; }//v1->nIMPr = 0;
		public short nASRr { get; set; }//v1->nASRr = 0;
		public short nTERr { get; set; }//v1->nTERr = 0;
		public short nMESOr { get; set; }//v1->nMESOr = 0;
		public short nPADx { get; set; }//v1->nPADx = 0;
		public short nMADx { get; set; }//v1->nMADx = 0;
		public short nIMDr { get; set; }//v1->nIMDr = 0;
		public short nPsdJump { get; set; }//v1->nPsdJump = 0;
		public short nPsdSpeed { get; set; }//v1->nPsdSpeed = 0;
		public short nOCr { get; set; }//v1->nOCr = 0;
		public short nDCr { get; set; }//v1->nDCr = 0;

		public PassiveSkillDataTemplate(int nSkillID) : base(nSkillID) { }
	}
}
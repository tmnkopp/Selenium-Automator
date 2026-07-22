namespace CyberScope.Automator
{
    public class InitializeAttempt
    { 
        #region CTOR 
        public InitializeAttempt()
        {
        } 
        #endregion 
        
        #region PROPS 
        public string Tab { get; set; } 
        public string Section { get; set; } 
        public object[] GetAsRow => new object[] { Tab, Section }; 
        #endregion

    }
}

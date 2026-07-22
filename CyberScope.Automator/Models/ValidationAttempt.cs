namespace CyberScope.Automator
{ 
    public class ValidationAttempt
    { 
        #region CTOR 
        public ValidationAttempt()
        {
        }
        public ValidationAttempt(string MetricXpath, string ErrorAttemptExpression )
        {  
            this.MetricXpath = MetricXpath;
            this.ErrorAttemptExpression = ErrorAttemptExpression; 
        }
        #endregion

        #region PROPS

        public string Tab { get; set; } = "";
        public string Section { get; set; } = "";
        public string MetricXpath { get; set; } = "";
        public string ErrorAttemptExpression { get; set; } = "";
        public string ExpectedError { get; set; } = "";
        public string ValidValue { get; set; } = "";
        public string AnswerProviderTypeName { get; set; }
        public object[] GetAsRow => new object[] { Section, MetricXpath, ErrorAttemptExpression, ExpectedError };

        #endregion

    }
}

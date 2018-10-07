namespace Exico.Shopify.Data.Domain.AppModels
{
    public class PlanRequirementAppModel
    {
        public int PlanId { get;  }
        public string OptionName { get; } = null;
        public string ExpectedValue { get; } = null;

        public PlanRequirementAppModel(int planId, string optionName, string expectedValue)
        {
            this.PlanId = planId;
            this.OptionName = optionName;
            this.ExpectedValue = expectedValue;

            if ((!string.IsNullOrEmpty(optionName) && string.IsNullOrEmpty(expectedValue))  ||
                (!string.IsNullOrEmpty(expectedValue) && string.IsNullOrEmpty(optionName))
                )
            {
                throw new System.Exception("If option name is present then expected value must be present and vice versa.");
            }

        }

        public override string ToString()
        {
            return $"PlanId = {PlanId} , Option Name = {OptionName ?? "null"} and Expected Value = {ExpectedValue ?? "null"}";
        }
    }
}
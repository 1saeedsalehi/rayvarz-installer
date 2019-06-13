

namespace RayvarzInstaller.ModernUI.App.Models
{
    public class StateInfo
    {
        public string StateName { get; set; }

        public int StepIndex { get; set; }

        public int StepCount { get; set; }

        public StateInfo()
        {
        }

        public StateInfo(string stateName, int stepIndex, int stepCount)
        {
            this.StateName = stateName;
            this.StepIndex = stepIndex > stepCount ? stepCount : stepIndex;
            this.StepCount = stepCount;
        }
    }
}

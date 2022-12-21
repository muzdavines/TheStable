using FIMSpace.Graph;
using UnityEngine;
using FIMSpace.Generating.Checker;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Field.Special
{
    public class PR_CustomTightPlacement : PR_TightPlacement
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "  Customized Tight Placement" : "Customized Tight Placement"; }
        public override string GetNodeTooltipDescription { get { return "Same as 'Tight Placement' node but this node allows for custom conditions check in 'OnCheck' output trigger (it's triggered on every fitting placement of TightPlacement algorithm). You can check field placement with custom arguments and send 'true' value to the 'StopChecking' port - it will result in stopping algorithm and calling 'Success' output trigger."; } }
        public override Vector2 NodeSize { get { return new Vector2(252, _EditorFoldout ? 254 : 218); } }
        public override int OutputConnectorsCount { get { return 3; } }
        public override int AllowedOutputConnectionIndex { get { return resultIndex; } }
        public override int HotOutputConnectionIndex { get { return 2; } }
        public override string GetOutputHelperText(int outputId = 0)
        {
            if (outputId == 0) return "Fail";
            else if (outputId == 1) return "Success";
            else return "On Check";
        }

        [Port(EPortPinType.Input, EPortValueDisplay.Default, 1)] public IntPort WantedAligns;
        [Port(EPortPinType.Input, EPortValueDisplay.HideValue, 1)] public BoolPort SkipIteration;
        [Port(EPortPinType.Input, EPortValueDisplay.HideValue, 1)] public BoolPort StopChecking;

        PlanGenerationPrint callPrint = null;
        public override void Execute(PlanGenerationPrint print, PlannerResult newResult)
        {
            callPrint = print;
            base.Execute(print, newResult);
        }

        protected override void OnPlacementIteration()
        {
            StopChecking.TriggerReadPort(true);

            if (StopChecking.GetPortValueSafe != null)
            {
                if (StopChecking.GetInputValue == true)
                {
                    forceBreak = true;
                }
            }
        }

        protected override void OnAfterPlacementIteration()
        {
            var preChe = field.LatestResult.Checker;
            field.LatestResult.Checker = checker;
            SkipIteration.TriggerReadPort(true);

            if (SkipIteration.GetPortValueSafe != null)
            {
                if (SkipIteration.GetInputValue == true)
                {
                    skipIteration = true;
                }
            }

            field.LatestResult.Checker = preChe;
        }

        protected override void OnFoundPlacementIteration()
        {
            if (callPrint != null)
            {
                GetContactCells(checker, alignToChecker, aligningDir, PushOutDistance, ref myCell, ref otherCell);
                if (field) ContactCell.ProvideFullCellData(myCell, field.LatestChecker, field.LatestResult);
                if (alignTo) AlignedToCell.ProvideFullCellData(otherCell, alignTo.LatestChecker, alignTo.LatestResult);
                field.LatestChecker.RootPosition = smallestRootPos;
                field.LatestChecker.RootRotation = smallestRootRot;
                
                // Provide data to the ports and call check trigger
                CallOtherExecutionWithConnector(2, callPrint);
            }
        }

        protected override int GetWantedAligns(CheckerField3D checker)
        {
            int wanted = WantedAligns.GetInputValue;
            if (wanted < 2) wanted = int.MaxValue;
            return wanted;
        }
    }
}
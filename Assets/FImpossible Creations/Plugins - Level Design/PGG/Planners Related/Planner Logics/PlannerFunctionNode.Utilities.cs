using FIMSpace.Generating.Planning.PlannerNodes;
using FIMSpace.Generating.Planning.PlannerNodes.FunctionNode;

namespace FIMSpace.Generating.Planning
{
    public partial class PlannerFunctionNode
    {
        /// <summary> When this is instance of function node inside some graph we need to sync it with project file function node graph </summary>
        private void SyncWithProjectFunctionFile()
        {
            if (ProjectFileParent != null)
            {
                localVars = ProjectFileParent.localVars;
                Rules = ProjectFileParent.Rules;
                Variables = ProjectFileParent.Variables;
                DisplayName = ProjectFileParent.DisplayName;
                nodeSize = ProjectFileParent.nodeSize;
            }
        }

        /// <summary> Defining input / output / parameters lists by searching contained nodes </summary>
        private void DefineDisplayPorts()
        {
            for (int i = 0; i < Rules.Count; i++)
            {
                if ( Rules[i] == null)
                {
                    CheckForNulls(Rules);
                    break;
                }

                Rules[i].ToRB().ParentNodesContainer = this;

                if (Rules[i] is PE_Start) { isExecutable = true; continue; }
                if (Rules[i] is FN_Input) { inputs.Add(Rules[i] as FN_Input); continue; }
                if (Rules[i] is FN_Parameter) { parameters.Add(Rules[i] as FN_Parameter); continue; }
                if (Rules[i] is FN_Output) { outputs.Add(Rules[i] as FN_Output); continue; }
            }
        }

        void RefreshLocalVariables()
        {
            if (localVars == null) localVars = new FieldPlanner.LocalVariables(this);
            localVars.RefreshList();
        }

        /// <summary> Getting procedures start execution node </summary>
        void DefineExecutionStartNode()
        {
            for (int i = 0; i < Rules.Count; i++)
            {
                if (Rules[i] is PE_Start)
                {
                    StartNode = Rules[i] as PE_Start;
                    break;
                }
            }
        }

        /// <summary> If this function node is containing other function nodes, let's call prepare on them too </summary>
        void PrepareSubFunctionNodes()
        {
            for (int i = 0; i < Rules.Count; i++)
            {
                PlannerFunctionNode func = Rules[i] as PlannerFunctionNode;
                if (func == null) continue;
                func.RefreshNodeParams();
                FGenerators.CheckForNulls(func.Rules);
                //func.PrepareInsideNodesPortInstances();
                //func.CheckPortsForNullConnections();
            }
        }

        void PreGeneratePrepareSubFunctionNodes()
        {
            for (int i = 0; i < Rules.Count; i++)
            {
                PlannerFunctionNode func = Rules[i] as PlannerFunctionNode;
                if (func == null) continue;
                func.PreGeneratePrepare();
            }
        }

        void CallReadOnDisplayedPorts()
        {
            for (int i = 0; i < DisplayPorts; i++)
            {
                var fPort = GetFunctionPort(i);
                var port = fPort.GetPort();
                if (port.IsOutput) continue;
                port.TriggerReadPort();
            }
        }

        void CallRefreshOnFunctionPorts()
        {
            for (int i = 0; i < DisplayPorts; i++)
            {
                var fPort = GetFunctionPort(i);
                fPort.RefreshValue();
            }
        }

        /// <summary> Prepare rules with the print </summary>
        void PrepareRules(PlanGenerationPrint print)
        {
            for (int i = 0; i < Rules.Count; i++)
            {
                Rules[i].ToRB().Prepare(print);
            }
        }

    }
}
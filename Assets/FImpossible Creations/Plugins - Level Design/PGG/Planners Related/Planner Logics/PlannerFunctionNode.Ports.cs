using FIMSpace.Generating.Planning.PlannerNodes;
using FIMSpace.Generating.Planning.PlannerNodes.FunctionNode;
using FIMSpace.Graph;
using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning
{
    public partial class PlannerFunctionNode
    {
        // Node params/ports references inside function node

        private List<FN_Input> inputs = new List<FN_Input>();
        private List<FN_Parameter> parameters = new List<FN_Parameter>();
        private List<FN_Output> outputs = new List<FN_Output>();

        #region Info why no List<FunctionPortRef> 

        // Ports displayed on node when added to some graph
        //public List<FunctionPortRef> NodePorts = new List<FunctionPortRef>(); !!! Rejected Approach, see below why

        // Unity property draw for node ports is limited and 
        // this is only way for supporting multiple ports
        // Lists / arrays are generating errors with
        // displaying the same ports multiple times
        // or throwing index errors 

        #endregion

        [HideInInspector, SerializeField] private FunctionPortRef Port0;
        [HideInInspector, SerializeField] private FunctionPortRef Port1;
        [HideInInspector, SerializeField] private FunctionPortRef Port2;
        [HideInInspector, SerializeField] private FunctionPortRef Port3;
        [HideInInspector, SerializeField] private FunctionPortRef Port4;
        [HideInInspector, SerializeField] private FunctionPortRef Port5;
        [HideInInspector, SerializeField] private FunctionPortRef Port6;
        [HideInInspector, SerializeField] private FunctionPortRef Port7;
        [HideInInspector, SerializeField] private FunctionPortRef Port8;
        [HideInInspector, SerializeField] private FunctionPortRef Port9;

        [HideInInspector, SerializeField] public int DisplayPorts = 0;


        public FunctionPortRef GetFunctionPort(int i)
        {
            switch (i)
            {
                #region Zero to 9
                case 0: return Port0;
                case 1: return Port1;
                case 2: return Port2;
                case 3: return Port3;
                case 4: return Port4;
                case 5: return Port5;
                case 6: return Port6;
                case 7: return Port7;
                case 8: return Port8;
                case 9: return Port9;
                    #endregion
            }

            if (i > 9) UnityEngine.Debug.Log("Exceed Limit of Node Ports!");

            return null;
        }

        public void SetFunctionPort(int i, FunctionPortRef port)
        {
            switch (i)
            {
                #region Zero to 9
                case 0: Port0 = port; return;
                case 1: Port1 = port; return;
                case 2: Port2 = port; return;
                case 3: Port3 = port; return;
                case 4: Port4 = port; return;
                case 5: Port5 = port; return;
                case 6: Port6 = port; return;
                case 7: Port7 = port; return;
                case 8: Port8 = port; return;
                case 9: Port9 = port; return;
                    #endregion
            }

            if (i > 9) UnityEngine.Debug.Log("Exceed Limit of Node Ports!");
        }

        private void RefreshDisplayPortInstances()
        {
#if UNITY_EDITOR
            sp_portsToDisplay.Clear();
#endif
            //DisplayPorts = 0;

            int portsCount = inputs.Count + outputs.Count + parameters.Count;
            if (DisplayPorts != portsCount)
            {
                if (DisplayPorts > portsCount) DisplayPorts = 0;
                else DisplayPorts = portsCount;
            }

            FillPorts(inputs);
            FillPorts(outputs);
            FillPorts(parameters);

            FillProprtiesList();

            inputPorts.Clear();
            outputPorts.Clear();

            for (int i = 0; i < DisplayPorts; i++)
            {
                var prt = GetFunctionPort(i).GetPort();

                if (prt.IsOutput) outputPorts.Add(prt);
                else inputPorts.Add(prt);
            }
        }

        void FillProprtiesList()
        {
#if UNITY_EDITOR
            //SerializedProperty sp = baseSerializedObject.FindProperty("NodePorts");
            for (int i = 0; i < DisplayPorts; i++)
            {
                SerializedProperty sp_ref = baseSerializedObject.FindProperty("Port" + i);

                switch (GetFunctionPort(i).ViewType)
                {
                    case EType.Int:
                        sp_portsToDisplay.Add(sp_ref.FindPropertyRelative("p_Int")); break;
                    case EType.Bool:
                        sp_portsToDisplay.Add(sp_ref.FindPropertyRelative("p_Bool")); break;
                    case EType.Number:
                        sp_portsToDisplay.Add(sp_ref.FindPropertyRelative("p_Float")); break;
                    case EType.Vector3:
                        sp_portsToDisplay.Add(sp_ref.FindPropertyRelative("p_Vector3")); break;
                    case EType.String:
                        sp_portsToDisplay.Add(sp_ref.FindPropertyRelative("p_String")); break;
                    case EType.Cell:
                        sp_portsToDisplay.Add(sp_ref.FindPropertyRelative("p_Cell")); break;
                    default:
                        break;
                }
            }
#endif
        }


        void FillPorts<T>(List<T> source) where T : PlannerRuleBase
        {
            for (int i = 0; i < source.Count; i++)
            {
                FunctionPortRef found = null;

                EPortPinType pinType;
                EType valueType;

                if (source[i] is FN_Output)
                {
                    FN_Output inp = source[i] as FN_Output;
                    pinType = EPortPinType.Output;
                    valueType = inp.OutputType;
                }
                else if (source[i] is FN_Input)
                {
                    FN_Input inp = source[i] as FN_Input;
                    pinType = EPortPinType.Input;
                    valueType = inp.InputType;
                }
                else
                {
                    FN_Parameter inp = source[i] as FN_Parameter;
                    pinType = EPortPinType.Input;
                    valueType = inp.InputType;
                }


                for (int p = 0; p < DisplayPorts; p++)
                {
                    var prt = GetFunctionPort(p);
                    if (prt == null) { DisplayPorts = 0; return; }

                    //if (prt.PinType == pinType)
                    if (prt.Parent == source[i])
                    {
                        found = prt;
                        break;
                    }
                }


                if (found == null)
                {
                    //UnityEngine.Debug.Log("not found gen new");
                    FunctionPortRef rf = new FunctionPortRef(source[i], valueType, pinType);
                    found = rf;
                    SetFunctionPort(DisplayPorts, rf);
                    DisplayPorts += 1;
                }

                if (found != null)
                {
                    NodePortBase prt = found.GetPort();
                    if (prt == null) UnityEngine.Debug.Log("Null port in " + DisplayName + " : " + found.DisplayName + " : " + pinType);

                    prt.PortType = pinType;
                    prt.ParentNode = this;
                    if (pinType == EPortPinType.Input) prt.LimitInConnectionsCount = 1;

                    prt.ParentNodeID = IndividualID;

                    prt.DisplayName = source[i].GetDisplayName();
                    prt._HelperFunctionsID = source[i].IndividualID;
                    if (prt.IsOutput) prt.ValueDisplayMode = EPortValueDisplay.NotEditable;

                    if (source[i] is FN_Parameter)
                    {
                        prt.SlotMode = EPortSlotDisplay.HidePort;
                        prt.ValueDisplayMode = EPortValueDisplay.AlwaysEditable;
                    }
                    else
                        prt.SlotMode = EPortSlotDisplay.Default;

                    if (prt.DefaultValue is Vector3)
                    {
                        if (prt.IsOutput) prt.NameDisplayMode = EPortNameDisplay.Default;
                        else
                        {
                            if (nodeSize.x < 240)
                                prt.ValueDisplayMode = EPortValueDisplay.HideValue;
                            else
                                prt.ValueDisplayMode = EPortValueDisplay.Default;
                        }
                    }
                    else
                    {
                        prt.NameDisplayMode = EPortNameDisplay.Default;
                    }
                }
            }
        }

#if UNITY_EDITOR
        public void OnNodeRemove(FGraph_NodeBase node)
        {
            DestroyImmediate(node, true);
            EditorUtility.SetDirty(node);
            if (ParentPlanner)
            {
                EditorUtility.SetDirty(ParentPlanner);
                if (ParentPlanner.ParentBuildPlanner) EditorUtility.SetDirty(ParentPlanner.ParentBuildPlanner);
            }
        }
#endif

    }
}
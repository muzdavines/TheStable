using FIMSpace.Generating.Planning.PlannerNodes;
using FIMSpace.Generating.Planning.PlannerNodes.FunctionNode;
using FIMSpace.Graph;
using UnityEngine;

namespace FIMSpace.Generating.Planning
{
    public partial class PlannerFunctionNode
    {

        [System.Serializable]
        public class FunctionPortRef
        {
            public PlannerRuleBase Parent;
            public string DisplayName = "";
            public int RootPortID = -1;
            public int RootPortHelpID = -1;
            public EType ViewType = EType.Int;
            public EPortPinType PinType = EPortPinType.Input;

            public IntPort p_Int = null;
            public BoolPort p_Bool = null;
            public FloatPort p_Float = null;
            public PGGVector3Port p_Vector3 = null;
            public PGGCellPort p_Cell = null;
            public PGGStringPort p_String = null;


            public FunctionPortRef(PlannerRuleBase parent, EType valueType, EPortPinType pinType)
            {
                Parent = parent;

                DisplayName = Parent.GetDisplayName();
                RootPortID = parent.IndividualID;

                ViewType = valueType;
                PinType = pinType;
                //UnityEngine.Debug.Log("Creat " + valueType);

                RefreshInstances();
            }


            void RefreshInstances(bool force = true)
            {
                if (force)
                {
                    p_Int = new IntPort();
                    p_Bool = new BoolPort();
                    p_Float = new FloatPort();
                    p_Vector3 = new PGGVector3Port();
                    p_Cell = new PGGCellPort();
                    p_String = new PGGStringPort();
                }
                else
                {
                    if (p_Int == null) p_Int = new IntPort();
                    if (p_Bool == null) p_Bool = new BoolPort();
                    if (p_Float == null) p_Float = new FloatPort();
                    if (p_Vector3 == null) p_Vector3 = new PGGVector3Port();
                    if (p_Cell == null) p_Cell = new PGGCellPort();
                    if (p_String == null) p_String = new PGGStringPort();
                }
            }


            internal NodePortBase GetPort()
            {
                NodePortBase prt = null;


                switch (ViewType)
                {
                    case EType.Int: prt = p_Int; break;
                    case EType.Bool: prt = p_Bool; break;
                    case EType.Number: prt = p_Float; break;
                    case EType.Vector3: prt = p_Vector3; break;
                    case EType.String: prt = p_String; break;
                    case EType.Cell: prt = p_Cell; break;
                }

                if (prt != null)
                {
                    if (Parent) prt.DisplayName = Parent.GetDisplayName();
                    else prt.DisplayName = DisplayName;
                }
                else
                {
                    RefreshInstances(false);
                }

                return prt;
            }

            internal void RefreshValue()
            {
                FN_Output fOut = Parent as FN_Output;

                if (fOut)
                {
                    fOut.OnStartReadingNode();
                    var port = fOut.GetFunctionOutputPort();
                    port.TriggerReadPort();
                    port.GetPortValueCall();

                    SetValueOf(port);
                }
                else if (Parent is FN_Input)
                {
                    FN_Input fInp = Parent as FN_Input;
                    fInp.OnStartReadingNode();

                    var port = GetPort();
                    ////UnityEngine.Debug.Log(port.DisplayName +" is connected ? " + port.Connections.Count);
                    port.TriggerReadPort(false);
                    port.GetPortValueCall();
                    fInp.SetValueOf(port);
                }
                else if (Parent is FN_Parameter)
                {
                    FN_Parameter fParam = Parent as FN_Parameter;
                    fParam.OnStartReadingNode();

                    var port = GetPort();
                    port.TriggerReadPort();
                    port.GetPortValueCall();

                    fParam.SetValue(port.GetPortValueSafe);
                }
            }

            void SetValueOf(NodePortBase p)
            {
                if (p == null) { UnityEngine.Debug.Log("Null port value!"); return; }
                object o = p.GetPortValueSafe;

                switch (ViewType)
                {
                    case EType.Int:
                        if (o != null)
                            p_Int.Value = (int)o;
                        break;
                    case EType.Bool:
                        if (o != null)
                            p_Bool.Value = (bool)o;
                        break;
                    case EType.Number:
                        if (o != null)
                            p_Float.Value = (float)o;
                        break;
                    case EType.Vector3:

                        if (o != null)
                        {
                            if (o is Vector3)
                            {
                                p_Vector3.Value = (Vector3)o;
                            }
                            else UnityEngine.Debug.Log("Inputting not Vector3! it's " + o.GetType() + " (" + o + ")");
                        }

                        break;
                    case EType.String:
                        if (o != null)
                            p_String.StringVal = (string)o;
                        break;
                    case EType.Cell:
                        if (p is PGGCellPort)
                        {
                            p_Cell.ProvideFullCellData(p as PGGCellPort);
                        }
                        break;
                }

            }

        }

    }

}
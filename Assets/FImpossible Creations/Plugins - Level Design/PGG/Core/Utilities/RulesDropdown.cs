#if UNITY_EDITOR
#if UNITY_2019_4_OR_NEWER
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using System;
using FIMSpace.Generating;

public class RulesDropdown : AdvancedDropdown
{
    public static FieldSpawner AddTo = null;
    public static DropElement Selected = null;
    public RulesDropdown(AdvancedDropdownState state) : base(state)
    {
        minimumSize = new Vector2(160, 220);
    }

    protected override void ItemSelected(AdvancedDropdownItem item)
    {
        base.ItemSelected(item);
        if (item is DropElement) Selected = item as DropElement;
    }

    public class DropElement : AdvancedDropdownItem
    {
        //public string name;
        public SpawnRuleBase toAdd;
        public AdvancedDropdownItem adItem;
        public DropElement parent;

        public DropElement(string name, SpawnRuleBase toAdd) : base(name)
        {
            this.name = name;
            this.toAdd = toAdd;
            adItem = new AdvancedDropdownItem(name);
            parent = null;
        }

        public SpawnRuleBase GetInstance()
        {
            return SpawnRuleBase.Instantiate(toAdd);
        }
    }

    public AdvancedDropdownItem FindParent(string name, List<AdvancedDropdownItem> categories)
    {
        for (int i = 0; i < categories.Count; i++)
        {
            if (categories[i].name == name) return categories[i];
        }

        return null;
    }

    string GetMenuName(Type type)
    {
        string name = type.ToString();
        name = name.Replace("FIMSpace.Generating.Rules.", "");
        return name.Replace('.', '/');
    }


    public static List<DropElement> allRules = new List<DropElement>();
    protected override AdvancedDropdownItem BuildRoot()
    {
        List<Type> types = FieldModification.GetDerivedTypes(typeof(SpawnRuleBase));

        #region Generating list of all rules


        if (allRules.Count > 0)
        {
            if (allRules[0].toAdd == null)
            {
                UnityEngine.Debug.Log("[PGG] Harmless Message: Detected Null in Nodes To Add list! Refreshing rules list to fix this.");
                allRules.Clear();
            }
        }

        if (allRules.Count <= 1)
            for (int i = 0; i < types.Count; i++)
            {
                Type type = types[i];

                if (type == typeof(SpawnRuleBase)) continue;
                string path = GetMenuName(type);
                if (string.IsNullOrEmpty(path)) continue;

                string name = path;
                SpawnRuleBase rule = SpawnRuleBase.CreateInstance(type) as SpawnRuleBase;
                if (rule != null) name = path.Replace(rule.GetType().Name, "") + rule.TitleName();

                allRules.Add(new DropElement(name, rule));
            }

        #endregion

        var root = new AdvancedDropdownItem("Spawn Rules");
        List<AdvancedDropdownItem> alreadyAddedParents = new List<AdvancedDropdownItem>();

        #region Managing parenting of rules

        for (int i = 0; i < allRules.Count; i++)
        {
            AdvancedDropdownItem parentDropItem = null;
            string[] parents = allRules[i].name.Split('/');

            for (int p = 0; p < parents.Length - 1; p++) // Checking if sections like "Transforming/Noise" are added
            { // -1 so we don't check for rule name as category
                parentDropItem = FindParent(parents[p], alreadyAddedParents);
                //UnityEngine.Debug.Log(allRules[i].name + "["+p+"] search for " + parents[p] + " = null? : " + (parentDropItem is null));

                if (parentDropItem == null) // Parent category not added yet to the list
                {
                    if (p == 0) // It's first depth parent name - so category in root
                    {
                        parentDropItem = new AdvancedDropdownItem(parents[p]);
                        root.AddChild(parentDropItem);
                        alreadyAddedParents.Add(parentDropItem);
                    }
                    else // It's another depth parent name - so child of other categories
                    {
                        var backParentItem = FindParent(parents[p - 1], alreadyAddedParents);

                        if (backParentItem != null)
                        {
                            parentDropItem = new AdvancedDropdownItem(parents[p]);
                            backParentItem.AddChild(parentDropItem);
                            alreadyAddedParents.Add(parentDropItem);
                        }
                    }
                }
            }

            if (parents.Length > 1)
                parentDropItem = FindParent(parents[parents.Length - 2], alreadyAddedParents);

            if (parentDropItem != null) // Found or new added
            {
                int lastSlash = allRules[i].name.LastIndexOf('/');
                if (lastSlash > 1 && lastSlash < allRules[i].name.Length - 1)
                {
                    string nodeName = allRules[i].name.Substring(lastSlash + 1, allRules[i].name.Length - (lastSlash + 1));
                    var ruleItem = new DropElement(nodeName, allRules[i].toAdd);
                    parentDropItem.AddChild(ruleItem);
                }
            }
        }

        #endregion

        return root;
    }

}

#endif
#endif

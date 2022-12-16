using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(LeagueSO))]
public class LeagueSOEditor : Editor
{
    
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
       
    }
    public void CreateNewTeam() {
        Character.Archetype[] possibleArchetypes = { Character.Archetype.Warrior, Character.Archetype.Wizard, Character.Archetype.Rogue };
        if ((target as LeagueSO).leagueLevel == 1) {
           possibleArchetypes = new Character.Archetype[] { Character.Archetype.Warrior, Character.Archetype.Wizard, Character.Archetype.Rogue, Character.Archetype.Thief, Character.Archetype.Thug, Character.Archetype.LightWizard, Character.Archetype.DarkWizard, Character.Archetype.Soldier, Character.Archetype.Mercenary};
        }
        if ((target as LeagueSO).leagueLevel == 1) {
            possibleArchetypes = new Character.Archetype[] { Character.Archetype.Warrior, Character.Archetype.Wizard, Character.Archetype.Rogue, Character.Archetype.Thief, Character.Archetype.Thug, Character.Archetype.LightWizard, Character.Archetype.DarkWizard, Character.Archetype.Soldier, Character.Archetype.Mercenary };
        }

        for (int i = 1; i < 5; i++) {
            var thisStable = new Stable() { stableName = Stable.stableNameList[i] };
            thisStable.heroes = new List<Character>();
            List<int> poss = new List<int>();
            for (int z = 1; z <= 15; z++) {
                Debug.Log("#InitStable#" + z);
                poss.Add(z);
            }
            for (int x = 0; x < 8; x++) {
                Character thisHero = new Character();
                thisHero.currentPosition = Position.NA;
                if (x < 5) {
                    thisHero.activeInLineup = true;
                    int randIndex = Random.Range(0, poss.Count);
                    int thisPos = poss[randIndex];
                    thisHero.currentPosition = (Position)(thisPos);
                    poss.RemoveAt(randIndex);
                }
                
                thisHero.GenerateCharacter((Character.Archetype)(Random.Range(5, 8)), 1);
                thisStable.heroes.Add(thisHero);
            }
            var GKHero = new Character();
            
            GKHero.GenerateCharacter(Character.Archetype.Goalkeeper);
            GKHero.currentPosition = Position.GK;
            GKHero.activeInLineup = true;
            thisStable.heroes.Add(GKHero);
            StableColorPalette colorPalette = Resources.Load<StableColorPalette>("Colors/League1");
            thisStable.primaryColor = colorPalette.randomPrimaryColor[Random.Range(0, colorPalette.randomPrimaryColor.Length)];
            thisStable.secondaryColor = colorPalette.randomSecondaryColor[Random.Range(0, colorPalette.randomSecondaryColor.Length)];
        }
    }
}

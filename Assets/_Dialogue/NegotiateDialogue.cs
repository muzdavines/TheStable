using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    [CreateAssetMenu]
public class NegotiateDialogue : ScriptableObject
{
    public string playerIntro = "Player Intro";
    public string npcIntro = "NPC Intro";
    public string[] npcPositive = { "I'm listening.", "Without respecting authority, we have nothing but chaos.", "Profits are good. I think we can work something out.", "I don't see why not! Let's drink to this new endeavor!" };
    public string[] npcNegative = { "No1.", "No2.", "No3.", "No4." };
    public string[] playerPositive = { "You seem like a man who knows how to conduct business.", "My stablemaster knows how important it is to ensure that...authority...is properly acknowledged.", "If your eminence would see fit to approve our proposal, I know we would have a...profitable...relationship.", "In light of our friendship, I'm sure my stablemaster would appreciate waiver of the market tariff." };
    public string[] playerNegative = { "Ahem...Ah...We are here to inquire about the possibility of potentially buying a stall in your marketplace.", "Bad2.", "Bad3.", "Bad4." };
    public string NPCSuccess = "NPC Success";
    public string NPCFail = "NPC Fail";
}

using System;
using System.Collections.Generic;
using UnityEngine;
public class TraitManager
{
    public int traitPoints { get; private set; } = 0;
    private Character character;
    private List<Trait> appliedTraits;
    public event Action OnTraitAdded;

    public event Action OnTraitRemoved;
    public TraitManager(Character character)
    {
        this.character = character;
        appliedTraits = new List<Trait>();
    }

    public bool AddTrait(Trait trait)
    {
        if (trait.IsCompletelyLearned())
        {
            Debug.LogWarning($"Trait '{trait.TraitName}' is already applied.");
            return false;
        }

        if (character.GetTraitPoints() < trait.Cost)
        {
            Debug.LogWarning("Not enough Trait Points to apply this Trait.");
            return false;
        }
        character.AddTrait(trait);
        character.playerData.LearnedTraits.Add(trait.TraitName);
        SaveSystem.SavePlayerData(character.playerData);

        OnTraitAdded?.Invoke();
        return true;
    }

    public void RemoveTrait(Trait trait)
    {
        character.RemoveTrait(trait);
    }

    public void ClearTraits()
    {
        foreach (var trait in new List<Trait>(character.Traits))
        {
            character.RemoveTrait(trait);
        }

        OnTraitRemoved?.Invoke();
    }

    public Trait GetTraitByName(string traitName)
    {
        return appliedTraits.Find(t => t.TraitName == traitName);
    }

    public Trait SearchTraitByName(string traitName)
    {
        return TraitRepository.Instance.allTraits.Find(t => t.TraitName == traitName);
    }

    public List<Trait> GetAllTraits()
    {
        return new List<Trait>(appliedTraits);
    }
}

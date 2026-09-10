using UnityEngine;

[CreateAssetMenu(
    fileName = "NewSpecies",
    menuName = "Animals/Species"
)]
public class SpeciesData : ScriptableObject
{
    [Header("Identity")]
    public string commonName;
    public string scientificName;

    [Header("Biology")]
    public float averageLifespanYears;
    public float averageAdultSizeInches;
}
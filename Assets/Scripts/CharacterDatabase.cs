using UnityEngine;

[System.Serializable]
public class Character
{
    public int id;
    public string name;
    public string category; // e.g., "Artist", "Politician", "Actor", "Criminal"
    public string description;
    public Sprite image;
}

[CreateAssetMenu(fileName = "CharacterDatabase", menuName = "Duel/Character Database")]
public class CharacterDatabase : ScriptableObject
{
    [SerializeField] public Character[] characters;

    /// <summary>
    /// Get a random subset of characters for the duel
    /// </summary>
    public Character[] GetRandomCharacterSet(int count)
    {
        if (count > characters.Length)
            count = characters.Length;

        Character[] selected = new Character[count];
        bool[] used = new bool[characters.Length];

        for (int i = 0; i < count; i++)
        {
            int randomIndex;
            do
            {
                randomIndex = Random.Range(0, characters.Length);
            } while (used[randomIndex]);

            used[randomIndex] = true;
            selected[i] = characters[randomIndex];
        }

        return selected;
    }

    /// <summary>
    /// Check if two characters share the same category (valid match)
    /// </summary>
    public bool AreCharactersMatched(Character char1, Character char2)
    {
        return char1 != null && char2 != null && char1.category == char2.category;
    }
}

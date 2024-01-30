using System;
using System.Linq;
using UnityEngine;
using TMPro;

public class GenerateRandom : MonoBehaviour
{
    [SerializeField] private TMP_InputField _inputField;
    
    public void getRandomKey()
    {
        string randomKey = generateRandomKey();
        // Debug.Log($"generateRandomCryptoKey: {randomKey}");
        _inputField.text = randomKey;
    }
    
    static string generateRandomKey()
    {
        var characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var charsArr = new char[8];
        var random = new System.Random();
        for(int i=0; i<charsArr.Length; i++)
        {
            charsArr[i] = characters[random.Next(characters.Length)];
        }
        return new string(charsArr);
    }
}

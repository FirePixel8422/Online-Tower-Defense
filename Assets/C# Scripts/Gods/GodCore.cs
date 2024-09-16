using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GodCore : MonoBehaviour
{
    public static GodCore Instance;
    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }



    public enum God
    {
        Athena,
        Hades,
        Zeus
    };


    public bool IsAthena
    {
        get
        {
            return god == God.Athena;
        }
    }

    public float zeusTroopStunChance;
    public int fails;
    public bool RandomStunChance()
    {
        if (god == God.Zeus && (Random.Range(0, 100f) < zeusTroopStunChance * (1 + fails)))
        {
            fails = 0;
            return true;
        }
        return false;
    }

    public bool IsHades
    {
        get
        {
            return god == God.Hades;
        }
    }
    public bool IsZeus
    {
        get
        {
            return god == God.Zeus;
        }
    }



    public God god;


    public virtual void UseDefensiveAbility()
    {

    }

    public virtual void UseOffensiveAbility()
    {

    }
}

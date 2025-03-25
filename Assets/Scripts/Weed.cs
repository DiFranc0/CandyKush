using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Classe para o doce individual
public class Weed : MonoBehaviour
{
    public int x;
    public int y;
    public int cannabisType;
    public bool isMatched = false;

    public GameObject smokeEffect;

    public GameObject pointsEffect;

    private Board board;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isSelected = false;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    public void Initialize(int xPos, int yPos, int type, Board boardRef)
    {
        x = xPos;
        y = yPos;
        cannabisType = type;
        board = boardRef;
    }

    private void OnMouseDown()
    {
        if (gameObject.activeInHierarchy)
        {
            board.SelectWeed(this);
        }
    }


    public void Select()
    {
        isSelected = true;
        spriteRenderer.color = new Color(originalColor.r * 0.7f, originalColor.g * 0.7f, originalColor.b * 0.7f);
    }

    public void Deselect()
    {
        isSelected = false;
        spriteRenderer.color = originalColor;
    }
}

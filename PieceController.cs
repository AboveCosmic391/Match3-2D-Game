using UnityEngine;
using System.Collections;

public class PieceController : MonoBehaviour
{
	
    // Array of colors
	private Color[] colors = new Color [6] { 
        Color.red, 
        Color.blue, 
        Color.green, 
        Color.white, 
        Color.yellow, 
        Color.magenta 
    };
	
    // Sprite renderer
	public SpriteRenderer sprite;
	

	private int index; // Index of the color
	private Vector2 coordinates; // Coordinates of the piece
	private bool destroyed; // If the piece is destroyed
	

	public int Index { get { return index; } set { index = value; } } // Index of the color
	public Vector2 Coordinates { get { return this.coordinates; } set { this.coordinates = value; } } // Coordinates of the piece
	public bool Destroyed { get { return destroyed; } set { destroyed = value; } } // If the piece is destroyed
	

    // Start method
	public void Start ()
	{
		index = Random.Range(0, colors.Length); // Random color
        Debug.Log("Index: " + index);
		sprite.color = colors[index]; // Set the color of the sprite
	}

    // check if the piece is a neighbour
	public bool IsNeighbour (PieceController otherPiece)
	{
		return Mathf.Abs(otherPiece.Coordinates.x - coordinates.x) + Mathf.Abs(otherPiece.Coordinates.y - coordinates.y) == 1; // Check if the piece is a neighbour
	}
	
}
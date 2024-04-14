using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameSceneManager : MonoBehaviour
{

	private int BOARD_WIDTH = 6; // Width of the board
	private int BOARD_HEIGHT = 5; // Height of the board
	private float PIECE_SPACING = 1.4f; // Spacing between pieces



	public Camera mainCamera; // Main camera
	public Text scoreText; // Score text
	public Text gameOverText; // Game over text
	public Transform levelContainer; // Level container

	public GameObject piecePrefab; // Piece prefab



	private int score; // Score
	private float gameTimer; // Game timer
	private bool gameOver; // If the game is over

	private PieceController[,] board; // Board
	private PieceController selectedPiece; // Selected piece


	public void Start ()
	{
		Time.timeScale = 1; // Set the time scale to 1
		gameOverText.enabled = false; // Disable the game over text

		BuildBoard(); // Build the board
	}

	public void Update ()
	{

        if(Input.GetKeyDown(KeyCode.Escape)) // If the player presses the escape key
        {
            Debug.Log("Quitting game");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reload the scene
            Debug.Log("Scene name: " + SceneManager.GetActiveScene().name);
        }

		if (gameOver) // If the game is over
		{
			if (Input.GetKeyDown("r")) // If the player presses R
			{
                Debug.Log("Restarting game");
				SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reload the scene
                Debug.Log("Scene name: " + SceneManager.GetActiveScene().name);
			}

			scoreText.enabled = false; // Disable the score text
			gameOverText.enabled = true; // Enable the game over text

			gameOverText.text = "Game over!\nTotal score: " + score + "\nPress R to restart"; // Set the game over text

			return; // Return
		}

		gameTimer += Time.deltaTime; // Increase the game timer

		scoreText.text = "Score: " + score; // Set the score text

		ProcessInput(); // Process the input
	}

	private void BuildBoard ()
	{
		board = new PieceController[BOARD_WIDTH, BOARD_HEIGHT]; // Create a new board

		for (int y = 0; y < BOARD_HEIGHT; y++) // Loop through the height of the board
		{
			for (int x = 0; x < BOARD_WIDTH; x++) // loop through the width of the board 
			{
				GameObject pieceObject = GameObject.Instantiate<GameObject>(piecePrefab); // Instantiate a new piece object
				pieceObject.transform.SetParent(levelContainer); // Set the parent of the piece object to the level container
				pieceObject.transform.localPosition = new Vector3 // Set the local position of the piece object
				(
					(-BOARD_WIDTH * PIECE_SPACING) / 2f + PIECE_SPACING / 2f + x * PIECE_SPACING, // X
					(-BOARD_HEIGHT * PIECE_SPACING) / 2f + PIECE_SPACING / 2f + y * PIECE_SPACING, // Y
					0 // Z
				);

				PieceController piece = pieceObject.GetComponent<PieceController>(); // Get the piece controller component
				piece.Coordinates = new Vector2(x, y); // Set the coordinates of the piece

				board[x,y] = piece; // Set the piece in the board
			}
		}
	}
	
	private void ProcessInput ()
	{
		if (Input.GetMouseButtonDown(0)) // If the player presses the left mouse button
		{
			Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); // Get the mouse position
			Collider2D hitCollider = Physics2D.OverlapPoint(mousePosition); // Get the collider at the mouse position

			if (hitCollider != null && hitCollider.gameObject.GetComponent<PieceController>() != null) // If the hit collider is not null and the hit collider has a piece controller component
			{
				PieceController hitPiece = hitCollider.gameObject.GetComponent<PieceController>(); // Get the piece controller component of the hit collider

				if (selectedPiece == null) // If there is no selected piece
				{
					selectedPiece = hitPiece; // Set the selected piece to the hit piece
					iTween.ScaleTo(selectedPiece.gameObject, iTween.Hash // Scale the selected piece
					(
						"scale", Vector3.one * 1.2f,
						"isLocal", true,
						"time", 0.3f
					));
				}
				else // If there is a selected piece
				{
					if (hitPiece == selectedPiece || !hitPiece.IsNeighbour(selectedPiece)) // If the hit piece is the selected piece or the hit piece is not a neighbour of the selected piece
					{
						iTween.ScaleTo(selectedPiece.gameObject, iTween.Hash // Scale the selected piece
						(
							"scale", Vector3.one,
							"isLocal", true,
							"time", 0.3f
						));
					}
					else if (hitPiece.IsNeighbour(selectedPiece)) // If the hit piece is a neighbour of the selected piece
					{
						AttemptMatch(selectedPiece, hitPiece); // Attempt to match the selected piece and the hit piece
					}

					selectedPiece = null; // Set the selected piece to null
				}
			}
		}
	}

	private void AttemptMatch (PieceController piece1, PieceController piece2)
	{
		StartCoroutine(AttemptMatchRoutine(piece1, piece2)); // Start the attempt match routine
	}

	private IEnumerator AttemptMatchRoutine (PieceController piece1, PieceController piece2)
	{
		iTween.Stop(piece1.gameObject); // Stop the iTween of the piece 1
		iTween.Stop(piece2.gameObject); // Stop the iTween of the piece 2

		piece1.transform.localScale = Vector3.one; // Set the scale of the piece 1 to one
		piece2.transform.localScale = Vector3.one; // Set the scale of the piece 2 to one

		Vector2 coordinates1 = piece1.Coordinates;  // Coordinates of the piece 1
		Vector2 coordinates2 = piece2.Coordinates; // Coordinates of the piece 2

		Vector3 position1 = piece1.transform.localPosition; // Position of the piece 1
		Vector3 position2 = piece2.transform.localPosition; // Position of the piece 2

		iTween.MoveTo(piece1.gameObject, iTween.Hash // Move the piece 1
		(
			"position", position2,
			"isLocal", true,
			"time", 0.5f
		));

		iTween.MoveTo(piece2.gameObject, iTween.Hash // Move the piece 2
		(
			"position", position1,
			"isLocal", true,
			"time", 0.5f
		));

		piece1.Coordinates = coordinates2; // Set the coordinates of the piece 1 to the coordinates of the piece 2
		piece2.Coordinates = coordinates1; // Set the coordinates of the piece 2 to the coordinates of the piece 1

		board[(int)piece1.Coordinates.x, (int)piece1.Coordinates.y] = piece1; // Set the piece 1 in the board
		board[(int)piece2.Coordinates.x, (int)piece2.Coordinates.y] = piece2; // Set the piece 2 in the board

		yield return new WaitForSeconds(0.5f); // Wait for 0.5 seconds

		List<PieceController> matchingPieces = CheckMatch(piece1); // Check the match of the piece 1
		if (matchingPieces.Count == 0) matchingPieces = CheckMatch(piece2); // If there are no matching pieces, check the match of the piece 2

		if (matchingPieces.Count < 3) // If there are less than 3 matching pieces
		{
			iTween.MoveTo(piece1.gameObject, iTween.Hash
			(
				"position", position1,
				"isLocal", true,
				"time", 0.5f
			));

			iTween.MoveTo(piece2.gameObject, iTween.Hash
			(
				"position", position2,
				"isLocal", true,
				"time", 0.5f
			));

			piece1.Coordinates = coordinates1; // Set the coordinates of the piece 1 to the coordinates 1
			piece2.Coordinates = coordinates2; // Set the coordinates of the piece 2 to the coordinates 2

			board[(int)piece1.Coordinates.x, (int)piece1.Coordinates.y] = piece1; // Set the piece 1 in the board
			board[(int)piece2.Coordinates.x, (int)piece2.Coordinates.y] = piece2; // Set the piece 2 in the board
 
			yield return new WaitForSeconds(1.0f); // Wait for 1 second

			CheckGameOver(); // Check the game over
		}
		else // If there are 3 or more matching pieces
		{
			foreach (PieceController piece in matchingPieces) // loop through the matching pieces
			{
				piece.Destroyed = true; // Set the piece to destroyed
				score += 100; // Increase the score by 100 
				iTween.ScaleTo(piece.gameObject, iTween.Hash 
				(
					"scale", Vector3.zero,
					"isLocal", true,
					"time", 0.25f
				));
			}

			yield return new WaitForSeconds(0.25f); // Wait for 0.25 seconds

			DropPieces(); // Drop the pieces
			AddPieces(); // Add the pieces

			yield return new WaitForSeconds(1.0f); // Wait for 1 second

			CheckGameOver(); // Check the game over
		}
	}

	private void DropPieces ()
	{
		for (int y = 0; y < BOARD_HEIGHT; y++) // Loop through the height of the board
		{
			for (int x = 0; x < BOARD_WIDTH; x++) // Loop through the width of the board
			{
				if (board[x,y].Destroyed) // If the piece is destroyed
				{
					// Found a destroyed piece! Must
					// make the ones above it fall
					bool dropped = false; // If the piece is dropped
					for (int j = y + 1; j < BOARD_HEIGHT && !dropped; j++) // Loop through the height of the board
					{
						if (!board[x,j].Destroyed) // If the piece is not destroyed
						{
							Vector2 coordinates1 = board[x,y].Coordinates; // Coordinates of the piece 1
							Vector2 coordinates2 = board[x,j].Coordinates; // Coordinates of the piece 2

							board[x,y].Coordinates = coordinates2; // Set the coordinates of the piece 1 to the coordinates of the piece 2
							board[x,j].Coordinates = coordinates1; // Set the coordinates of the piece 2 to the coordinates of the piece 1

							iTween.MoveTo(board[x,j].gameObject, iTween.Hash
							(
								"position", board[x,y].transform.localPosition,
								"isLocal", true,
								"time", 0.25f
							));

							board[x,y].transform.localPosition = board[x,j].transform.localPosition; // Set the local position of the piece 1 to the local position of the piece 2

							PieceController fallingPiece = board[x,j]; // Falling piece
							board[x,j] = board[x,y]; // Set the piece in the board
							board[x,y] = fallingPiece; // Set the falling piece in the board

							dropped = true; // Set dropped to true
						}
					}
				}
			}
		}
	}

	private void AddPieces ()
	{
		int firstY = -1; // First Y; 

		for (int y = 0; y < BOARD_HEIGHT; y++) // Loop through the height of the board
		{
			for (int x = 0; x < BOARD_WIDTH; x++) // Loop through the width of the board
			{
				if (board[x,y].Destroyed) // If the piece is destroyed
				{
					// Found a destroyed piece! Add a new one
					
					if (firstY == -1) firstY = y; // If the first Y is -1, set the first Y to Y

					PieceController oldPiece = board[x,y]; // Old piece

					GameObject newPieceObject = GameObject.Instantiate<GameObject>(piecePrefab); // Instantiate a new piece object
					newPieceObject.transform.SetParent(levelContainer); // Set the parent of the new piece object to the level container
					newPieceObject.transform.localPosition = new Vector3 
					(
						oldPiece.transform.position.x,
						6.0f,
						0
					);

					iTween.MoveTo(newPieceObject, iTween.Hash
					(
						"position", oldPiece.transform.localPosition,
						"isLocal", true,
						"time", 0.25f,
						"delay", 0.150f * (y - firstY)
					));

					PieceController piece = newPieceObject.GetComponent<PieceController>(); // Get the piece controller component of the new piece object
					piece.Coordinates = oldPiece.Coordinates; // Set the coordinates of the piece

					board[x,y] = piece; // Set the piece in the board

					GameObject.Destroy(oldPiece.gameObject); // Destroy the old piece
				}
			}
		}
	}

	private List<PieceController> CheckMatch (PieceController piece)
	{
		List<PieceController> matchingNeighbours = new List<PieceController>(); // Matching neighbours

		int x = 0; // X
		int y = (int) piece.Coordinates.y; // Y
		bool reachedPiece = false; // If the piece is reached

		// Checks for matches horizontally

		while (x < BOARD_WIDTH) // Loop through the width of the board
		{
			if (!board[x,y].Destroyed && board[x,y].Index == piece.Index) // If the piece is not destroyed and the index of the piece is the index of the piece
			{
				matchingNeighbours.Add(board[x,y]); // Add the piece to the matching neighbours
				if (board[x,y] == piece) reachedPiece = true; // If the piece is the piece, set reached piece to true
			}
			else // If the piece is destroyed or the index of the piece is not the index of the piece
			{
				if (!reachedPiece) matchingNeighbours.Clear(); // Didn't reach the matching piece
				else if (matchingNeighbours.Count >= 3) return matchingNeighbours; // Reached the matching piece and got enough pieces
				else matchingNeighbours.Clear(); // Reached matching piece but got few pieces
			}

			x++; // Increase X
		}

		if (matchingNeighbours.Count >= 3) return matchingNeighbours; // If there are 3 or more matching neighbours, return the matching neighbours

		x = (int) piece.Coordinates.x; // X
		y = 0; // Y 
		reachedPiece = false; // Set reached piece to false
		matchingNeighbours.Clear(); // Clear the matching neighbours

		// Checks for matches vertically

		while (y < BOARD_HEIGHT) // Loop through the height of the board
		{
			if (!board[x,y].Destroyed && board[x,y].Index == piece.Index) // If the piece is not destroyed and the index of the piece is the index of the piece
			{
				matchingNeighbours.Add(board[x,y]); // Add the piece to the matching neighbours
				if (board[x,y] == piece) reachedPiece = true; // If the piece is the piece, set reached piece to true
			}
			else // If the piece is destroyed or the index of the piece is not the index of the piece
			{
				if (!reachedPiece) matchingNeighbours.Clear(); // Didn't reach the matching piece
				else if (matchingNeighbours.Count >= 3) return matchingNeighbours; // Reached the matching piece and got enough pieces
				else matchingNeighbours.Clear(); // Reached matching piece but got few pieces
			}

			y++; // Increase Y
		}

		return matchingNeighbours; // Return the matching neighbours
	}

	private void CheckGameOver ()
	{
		int possibleMatches = 0; // Possible matches

		for (int y = 0; y < BOARD_HEIGHT; y++) // Loop through the height of the board
		{
			for (int x = 0; x < BOARD_WIDTH; x++) // Loop through the width of the board
			{
				PieceController piece1 = board[x, y]; // Piece 1
				Vector2 coordinates1 = piece1.Coordinates; // Coordinates of the piece 1

				PieceController piece2; // Piece 2
				Vector2 coordinates2; // Coordinates of the piece 2

				// Checks for horizontal swap

				if (x < BOARD_WIDTH - 1) // If X is less than the width of the board - 1
				{
					piece2 = board[x + 1, y]; // Piece 2
					coordinates2 = piece2.Coordinates; // Coordinates of the piece 2

					piece1.Coordinates = coordinates2; // Set the coordinates of the piece 1 to the coordinates of the piece 2
					piece2.Coordinates = coordinates1; // Set the coordinates of the piece 2 to the coordinates of the piece 1

					board[(int)piece1.Coordinates.x, (int)piece1.Coordinates.y] = piece1; // Set the piece 1 in the board
					board[(int)piece2.Coordinates.x, (int)piece2.Coordinates.y] = piece2; // Set the piece 2 in the board

					if (CheckMatch(piece1).Count >= 3 || CheckMatch(piece2).Count >= 3) // If the count of the matching pieces of the piece 1 is greater than or equal to 3 or the count of the matching pieces of the piece 2 is greater than or equal to 3
					{
						possibleMatches++; // Increase the possible matches
					}

					piece1.Coordinates = coordinates1; // Set the coordinates of the piece 1 to the coordinates 1
					piece2.Coordinates = coordinates2; // Set the coordinates of the piece 2 to the coordinates 2

					board[(int)piece1.Coordinates.x, (int)piece1.Coordinates.y] = piece1; // Set the piece 1 in the board
					board[(int)piece2.Coordinates.x, (int)piece2.Coordinates.y] = piece2; // Set the piece 2 in the board
				}

				// Checks for vertical swap

				if (y < BOARD_HEIGHT - 1) // If Y is less than the height of the board - 1
				{
					piece2 = board[x, y + 1]; // Piece 2
					coordinates2 = piece2.Coordinates; // Coordinates of the piece 2

					piece1.Coordinates = coordinates2; // Set the coordinates of the piece 1 to the coordinates of the piece 2
					piece2.Coordinates = coordinates1; // Set the coordinates of the piece 2 to the coordinates of the piece 1

					board[(int)piece1.Coordinates.x, (int)piece1.Coordinates.y] = piece1; // Set the piece 1 in the board
					board[(int)piece2.Coordinates.x, (int)piece2.Coordinates.y] = piece2; // Set the piece 2 in the board

					if (CheckMatch(piece1).Count >= 3 || CheckMatch(piece2).Count >= 3) // If the count of the matching pieces of the piece 1 is greater than or equal to 3 or the count of the matching pieces of the piece 2 is greater than or equal to 3
					{
						possibleMatches++; // Increase the possible matches
					}

					piece1.Coordinates = coordinates1; // Set the coordinates of the piece 1 to the coordinates 1
					piece2.Coordinates = coordinates2; // Set the coordinates of the piece 2 to the coordinates 2

					board[(int)piece1.Coordinates.x, (int)piece1.Coordinates.y] = piece1; // Set the piece 1 in the board
					board[(int)piece2.Coordinates.x, (int)piece2.Coordinates.y] = piece2; // Set the piece 2 in the board
				}
			}
		}

		if (possibleMatches == 0) // If there are no possible matches
		{
			OnGameOver(); // On game over
		}
	}

	private void OnGameOver () // On game over
	{
		gameOver = true; // Set the game over to true
	}

}
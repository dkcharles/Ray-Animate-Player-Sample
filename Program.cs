/*******************************************************************************************
*
*   raylib [textures] example - Texture loading and drawing a part defined by a rectangle
*
*   This example has been created using raylib 1.3 (www.raylib.com)
*   raylib is licensed under an unmodified zlib/libpng license (View raylib.h for details)
*
*   Copyright (c) 2014 Ramon Santamaria (@raysan5)
*
********************************************************************************************/
using System;
using System.Numerics;
using static Raylib_cs.Raylib;
using Raylib_cs;

namespace Examples.Textures;

public class SpriteAnim
{
    public const int MaxFrameSpeed = 15;
    public const int MinFrameSpeed = 1;

    public static int Main()
    {
        // Initialization
        //--------------------------------------------------------------------------------------
        const int screenWidth = 1200;
        const int screenHeight = 800;

        InitWindow(screenWidth, screenHeight, "raylib [texture] example - texture rectangle");

        // NOTE: Textures MUST be loaded after Window initialization (OpenGL context is required)
        Texture2D scarfy = LoadTexture("resources/scarfy.png");

        if (scarfy.Id == 0)
        {
            Console.WriteLine("Failed to load texture");
            return -1;
        }

        // Create a player instance
        Player player = new Player(scarfy, new Vector2(350.0f, 280.0f), screenWidth, screenHeight);

        SetTargetFPS(60);
        //--------------------------------------------------------------------------------------
        Console.WriteLine("Press RIGHT/LEFT arrows to change speed!");
        // Main game loop
        while (!WindowShouldClose())
        {
            // Update
            //----------------------------------------------------------------------------------
            player.Update();
            //----------------------------------------------------------------------------------

            // Draw
            //----------------------------------------------------------------------------------
            BeginDrawing();
            ClearBackground(Color.RayWhite);

            // Draw the texture and frame rectangle for debugging
            DrawTexture(scarfy, 15, 40, Color.White);
            DrawRectangleLines(15, 40, scarfy.Width, scarfy.Height, Color.Lime);
            DrawRectangleLines(
                15 + (int)player.FrameRec.X,
                40 + (int)player.FrameRec.Y,
                (int)player.FrameRec.Width,
                (int)player.FrameRec.Height,
                Color.Red
            );

            // Display frame speed
            DrawText("FRAME SPEED: ", 165, 210, 10, Color.DarkGray);
            DrawText($"{player.FramesSpeed:2F} FPS", 575, 210, 10, Color.DarkGray);
            DrawText("PRESS RIGHT/LEFT KEYS to CHANGE SPEED!", 290, 240, 10, Color.DarkGray);

            // Draw frame speed indicators
            for (int i = 0; i < MaxFrameSpeed; i++)
            {
                if (i < player.FramesSpeed)
                {
                    DrawRectangle(250 + 21 * i, 205, 20, 20, Color.Red);
                }
                DrawRectangleLines(250 + 21 * i, 205, 20, 20, Color.Maroon);
            }

            // Draw the player
            player.Draw();
            DrawText("(c) Scarfy sprite by Eiden Marsal", screenWidth - 200, screenHeight - 20, 10, Color.Gray);

            // Draw ground
            DrawRectangleRec(player.Ground, Color.Brown);

            EndDrawing();
            //----------------------------------------------------------------------------------
        }

        // De-Initialization
        //--------------------------------------------------------------------------------------
        UnloadTexture(scarfy);

        CloseWindow();
        //--------------------------------------------------------------------------------------

        return 0;
    }
}

public class Player
{
    public Vector2 Position { get; private set; }
    public Vector2 Velocity { get; private set; }
    public Rectangle FrameRec { get; private set; }
    public Rectangle Ground { get; private set; }
    public int FramesSpeed { get; private set; }
    public bool FacingRight { get; private set; }
    public bool IsOnGround { get; private set; }

    private const float Gravity = 0.5f;
    private const float JumpVelocity = -10.0f;
    private const float HorizontalAcceleration = 0.1f;
    private const float Friction = 0.1f;
    private int currentFrame;
    private int framesCounter;
    private Texture2D texture;
    private int screenWidth;
    private int screenHeight;

    public Player(Texture2D texture, Vector2 startPosition, int screenWidth, int screenHeight)
    {
        this.texture = texture;
        Position = startPosition;
        Velocity = new Vector2(0.0f, 0.0f);
        FrameRec = new Rectangle(0.0f, 0.0f, (float)texture.Width / 6, (float)texture.Height);
        Ground = new Rectangle(0.0f, screenHeight - 50, screenWidth, 50);
        FramesSpeed = 8;
        FacingRight = true;
        IsOnGround = false;
        this.screenWidth = screenWidth;
        this.screenHeight = screenHeight;
    }

    public void Update()
    {
        framesCounter++;

        // Update animation frame based on frame speed
        if (framesCounter >= (60 / FramesSpeed))
        {
            framesCounter = 0;
            currentFrame++;

            if (currentFrame > 5)
            {
                currentFrame = 0;
            }

            FrameRec = new Rectangle((float)currentFrame * (float)texture.Width / 6, FrameRec.Y, FrameRec.Width, FrameRec.Height);
        }

        // Adjust frame speed with arrow keys
        if (IsKeyPressed(KeyboardKey.Right))
        {
            FramesSpeed++;
        }
        else if (IsKeyPressed(KeyboardKey.Left))
        {
            FramesSpeed--;
        }

        FramesSpeed = Math.Clamp(FramesSpeed, SpriteAnim.MinFrameSpeed, SpriteAnim.MaxFrameSpeed);

        // Move sprite with cursor keys and update direction
        if (IsKeyDown(KeyboardKey.D))
        {
            Velocity = new Vector2(Velocity.X + HorizontalAcceleration, Velocity.Y);
            FacingRight = true;
        }
        else if (IsKeyDown(KeyboardKey.A))
        {
            Velocity = new Vector2(Velocity.X - HorizontalAcceleration, Velocity.Y);
            FacingRight = false;
        }
        else
        {
            // Apply friction to slow down the player when no keys are pressed
            if (Velocity.X > 0)
            {
                Velocity = new Vector2(Velocity.X - Friction, Velocity.Y);
                if (Velocity.X < 0) Velocity = new Vector2(0, Velocity.Y);
            }
            else if (Velocity.X < 0)
            {
                Velocity = new Vector2(Velocity.X + Friction, Velocity.Y);
                if (Velocity.X > 0) Velocity = new Vector2(0, Velocity.Y);
            }
        }

        // Apply gravity to the player's vertical velocity
        Velocity = new Vector2(Velocity.X, Velocity.Y + Gravity);

        // Update the player's position based on the current velocity
        Position += Velocity;

        // Check for collision with the ground
        if (Position.Y + FrameRec.Height >= Ground.Y)
        {
            // If the player is on the ground, set the position to be exactly on top of the ground
            Position = new Vector2(Position.X, Ground.Y - FrameRec.Height);
            
            // Reset the vertical velocity to 0 to stop falling
            Velocity = new Vector2(Velocity.X, 0);
            
            // Set the flag indicating the player is on the ground
            IsOnGround = true;
        }
        else
        {
            // If the player is not on the ground, set the flag to false
            IsOnGround = false;
        }

        // Check if the space bar is pressed and the player is on the ground
        if (IsOnGround && IsKeyPressed(KeyboardKey.Space))
        {
            // Apply an upward velocity to simulate a jump
            Velocity = new Vector2(Velocity.X, JumpVelocity);
            
            // Set the flag indicating the player is no longer on the ground
            IsOnGround = false;
        }

        // Ensure the player stays within the window boundaries
        Position = new Vector2(
            Math.Clamp(Position.X, 0, screenWidth - FrameRec.Width), // Clamp the X position
            Math.Clamp(Position.Y, 0, screenHeight - FrameRec.Height) // Clamp the Y position
        );
    }

    public void Draw()
    {
        // Create a source rectangle for the texture
        Rectangle sourceRec = FrameRec;
        
        // Flip the texture horizontally if the player is facing left
        if (!FacingRight)
        {
            sourceRec.Width = -FrameRec.Width;
        }
        
        // Draw the texture at the player's position
        DrawTextureRec(texture, sourceRec, Position, Color.White);
    }
}
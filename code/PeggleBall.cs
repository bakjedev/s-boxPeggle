using Sandbox;
using System;
using System.Collections.Generic;
using System.Numerics;
public sealed class PeggleBall : Component, Component.ICollisionListener
{
	[Property]
	public Rigidbody body { get; set; }
	[Property]
	public PointLight pointLight { get; set; }
	[Property]
	public CameraComponent camera { get; set; }
	[Property]
	public GameObject targetBall { get; set; }

	public List<GameObject> targetBalls = new();

	private Vector3 startPos;
	private bool resetting = false;
	private bool won = false;


	protected override void OnStart()
	{
		startPos = Transform.Position;
		
		SpawnQuintuple( new Vector3( 0, 400, 200 ) );

		// Spawn all balls from the list
		foreach ( var o in targetBalls )
		{
			o.NetworkSpawn();
		}
		
	}

	protected override void OnUpdate()
	{
		// Calculate the direction from the ball to the mouse position
		var ballScreenPos = camera.PointToScreenPixels(Transform.Position);
		var pointingToCursor = new Vector2( Mouse.Position.x - ballScreenPos.x, -(Mouse.Position.y - ballScreenPos.y) ).Normal;

		// Draw a line showing the direction of the ball
		Gizmo.Draw.Line( Transform.Position, Transform.Position + new Vector3(pointingToCursor.x, 0, pointingToCursor.y) * 50.0f );

		if (Input.Pressed("attack1") && !body.Gravity)
		{
			body.Gravity = true;


			body.Velocity = new Vector3( pointingToCursor.x, 0, pointingToCursor.y ) * 1000;
		}

		// If ball has touched the floor
		if ( resetting )
		{
			// Stop the ball
			body.LinearDamping += 100000.0f * Time.Delta;
			
			// Dim the light
			pointLight.Radius -= 3000.0f * Time.Delta;
			
			// Once the light is fully dimmed
			if ( pointLight.Radius <= 0 )
			{
				// Reset the ball back to the top
				resetting = false;
				body.LinearDamping = 0;
				ResetBall();
			}
		}
		else if ( pointLight.Radius <= 2000.0f )
		{
			// Bring the ligth back to its normal radius
			pointLight.Radius += 1000.0f * Time.Delta;
		}

		// Checking if all target balls have been hit
		if (targetBalls.Count == 0 && !won)
		{
			SoundEvent shootSound = Cloud.SoundEvent( "mdlresrc.toolgunshoot" );
			Sound.Play( shootSound, Transform.Position );
			won = true;
		}
	}
	public void OnCollisionStart( Collision o )
	{
	}

	public void OnCollisionStop( CollisionStop o )
	{
		// Set target ball's tint to blue.
		var modelRenderer = o.Other.GameObject.Components.GetInDescendantsOrSelf<ModelRenderer>();
		if ( modelRenderer != null && o.Other.GameObject.Tags.Has( "target" ) )
		{
			modelRenderer.Tint = Color.Blue;
			targetBalls.Remove( o.Other.GameObject );
		}

		// If ball touches the floor.
		if ( o.Other.GameObject.Tags.Has( "end" )  && !resetting)
		{
			resetting = true;
		}
	}

	public void OnCollisionUpdate( Collision o )
	{

	}

	private void ResetBall()
	{
		body.Gravity = false;
		Transform.Position = startPos;
		body.Velocity = Vector3.Zero;
	}

	// Add 5 target balls to the targetBalls list (3 bottom, 2 on top)
	private void SpawnQuintuple(Vector3 targetPos)
	{
		var o1 = targetBall.Clone();
		o1.Transform.Position = targetPos;
		targetBalls.Add( o1 );

		var o2 = targetBall.Clone();
		o2.Transform.Position = targetPos + new Vector3( 50, 0, 0 );
		targetBalls.Add( o2 );

		var o3 = targetBall.Clone();
		o3.Transform.Position = targetPos + new Vector3( -50, 0, 0 );
		targetBalls.Add( o3 );

		var o4 = targetBall.Clone();
		o4.Transform.Position = targetPos + new Vector3( -25, 0, 25 );
		targetBalls.Add( o4 );

		var o5 = targetBall.Clone();
		o5.Transform.Position = targetPos + new Vector3( 25, 0, 25 );
		targetBalls.Add( o5 );
	}
}

﻿using UnityEngine;
using Mirror;

public class PhysicsSim : MonoBehaviour
{
    PhysicsScene physicsScene;
    PhysicsScene2D physicsScene2D;

    bool simulatePhysicsScene;
    bool simulatePhysicsScene2D;


    private void Awake()
    {
        if (NetworkServer.active)
        {
            physicsScene = gameObject.scene.GetPhysicsScene();
            simulatePhysicsScene = physicsScene.IsValid() && physicsScene != Physics.defaultPhysicsScene;

            physicsScene2D = gameObject.scene.GetPhysicsScene2D();
            simulatePhysicsScene2D = physicsScene2D.IsValid() && physicsScene2D != Physics2D.defaultPhysicsScene;
        }
        else
        {
            enabled = false;
        }
    }

    private void FixedUpdate()
    {
        if (!NetworkServer.active) return;

        if (simulatePhysicsScene)
            physicsScene.Simulate(Time.fixedDeltaTime);

        if (simulatePhysicsScene2D)
            physicsScene2D.Simulate(Time.fixedDeltaTime);
    }
}
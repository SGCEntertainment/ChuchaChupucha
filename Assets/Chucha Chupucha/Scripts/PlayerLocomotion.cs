using UnityEngine;
using Fusion;

public class PlayerLocomotion : NetworkBehaviour
{
    protected NetworkCharacterControllerPrototype networkCharacterControllerPrototype;

    [Networked]
    Vector3 MovementDirection { get; set; }
    [SerializeField] float maxStrength;

    private GameObject _mainCamera;

    [Networked] private SoldierInput.NetworkInputData Inputs { get; set; }

    public void Awake()
    {
        CacheComponents();
    }

    public override void Spawned()
    {
        CacheComponents();
    }

    private void CacheComponents()
    {
        if (!networkCharacterControllerPrototype)
        {
            networkCharacterControllerPrototype = GetComponent<NetworkCharacterControllerPrototype>();
        }

        if (!_mainCamera)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }

    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();

        if (GetInput(out SoldierInput.NetworkInputData input))
        {
            //
            // Copy our inputs that we have received, to a [Networked] property, so other clients can predict using our
            // tick-aligned inputs. This is the core of the Client Prediction system.
            //
            Inputs = input;
        }

        UpdatePosition(Inputs);
    }

    private void UpdatePosition(SoldierInput.NetworkInputData input)
    {
        Vector3 inputNormalized = input.moveDirection.normalized;
        MovementDirection = new Vector3(inputNormalized.x, 0, inputNormalized.y);
        networkCharacterControllerPrototype.Move(MovementDirection * maxStrength);
    }
}

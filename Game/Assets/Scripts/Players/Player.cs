using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using Grid;
using UI;
using UnityEngine;

[System.Serializable]
public abstract class Player : MonoBehaviour
{
    public uint money;
    public bool placing;

    [SerializeField] protected TileManager tileManager;

    [SerializeField] protected TileCursor tileCursor;
    [SerializeField] private KeyCode[] _unitKeys;
    [SerializeField] private string[] _buttonNames;
    [SerializeField] private string _placeButtonName;

    
    [SerializeField] private string horizontalString = "Horizontal";
    [SerializeField] private string verticalString = "Vertical";
    [SerializeField] private SpriteRenderer unitRender;
    
    [SerializeField] private SelectUnits unitSelector;
    [SerializeField] private KeyCode _placeUnit = KeyCode.Return;
    [SerializeField] protected GameObject[] _unitsToSpawn;
    
    
    private Timer _canMoveTimer;
    public bool playAlienCursorNoise;

    protected int currentUnitIndex;
    protected bool placedUnit;

    private void Awake()
    {
        _canMoveTimer = new Timer(0.15f);
        unitRender.sprite = _unitsToSpawn[currentUnitIndex].GetComponent<SpriteRenderer>().sprite;

    }
    
    protected virtual void Update()
    {
        unitRender.gameObject.transform.position = tileCursor.transform.position;
        if (_canMoveTimer.IsFinished())
        {
            _canMoveTimer.Reset();
        }
        
        _canMoveTimer.UpdateTime();
        
        for (var i = 0; i < _unitKeys.Length; ++i)
        {
            if (!Input.GetKey(_unitKeys[i]) && !Input.GetButton(_buttonNames[i])) continue;

            currentUnitIndex = i;
            unitSelector.SelectUnit(currentUnitIndex);
            unitRender.sprite = _unitsToSpawn[currentUnitIndex].GetComponent<SpriteRenderer>().sprite;
            
            break;
        }

        ControlPlacementCursor();
    }

    private void ControlPlacementCursor()
    {
        bool startedHoldLeft = false;
        bool startedHoldRight = false;
        bool startedHoldUp = false;
        bool startedHoldDown = false;
        
        
        
        if (Input.GetAxisRaw(horizontalString) > -0.2f
            && Input.GetAxisRaw(horizontalString) < 0.2f
            && Input.GetAxisRaw(verticalString) > -0.2f
            && Input.GetAxisRaw(verticalString) < 0.2f)
        {
            StopAllCoroutines();
        }

        if (!Input.GetKey(_placeUnit) && !Input.GetButton(_placeButtonName))
        {
            placedUnit = false;
        }
        
        // Place unit down.
        if ((Input.GetKey(_placeUnit) || Input.GetButton(_placeButtonName)) && !placedUnit)
        {
            TryPlaceUnit();
        }

        if (!_canMoveTimer.IsFinished()) return;

        if (Input.GetAxisRaw(horizontalString) < -0.2f)
        {
            StopAllCoroutines();
            StartCoroutine(StartMove(horizontalString, -1, 0, 0.5f, 0.05f));
        }

        if (Input.GetAxisRaw(horizontalString) > 0.2f)
        {
            StopAllCoroutines();
            StartCoroutine(StartMove(horizontalString, 1, 0, 0.5f, 0.05f));
        }

        if (Input.GetAxisRaw(verticalString) > 0.2f)
        {
            StopAllCoroutines();
            StartCoroutine(StartMove(verticalString, 0, -1, 0.5f, 0.05f));
        }

        if (Input.GetAxisRaw(verticalString) < -0.2f)
        {
            StopAllCoroutines();
            StartCoroutine(StartMove(verticalString, 0, 1, 0.5f, 0.05f));
        }

        
    }

    protected abstract void TryPlaceUnit();

    private IEnumerator StartMove(string key, int moveRow, int moveCol,
        float waitAfterFirstMove,
        float waitPastFirstMove)
    {
        PlayCursorSoundFX();
        bool first = true;
        tileCursor.Move(moveRow, moveCol);
        while (Input.GetAxisRaw(key) > 0.2f || Input.GetAxisRaw(key) < -0.2f)
        {
            if (first)
            {
                yield return new WaitForSeconds(waitAfterFirstMove);
                first = false;
            }
            else
            {
                yield return new WaitForSeconds(waitPastFirstMove);
            }

            tileCursor.Move(moveRow, moveCol);
            yield return null;
        }
    }

    private void OpenPlacementMode()
    {
        placing = true;
        tileCursor.Show();
    }

    protected void ClosePlacementMode()
    {
        placing = false;
        tileCursor.Hide();
    }

    public void OpenMultiPlacementMode(ShopUnit unit, int number)
    {
        // Attacker does not have enough money to place the units
        if (money < unit.Cost * number)
            return;
        OpenPlacementMode();
    }

    public void OpenSinglePlacementMode(ShopUnit unit)
    {
        // Attacker does not have enough money to place the unit
        if (money < unit.Cost)
            return;
        OpenPlacementMode();
    }

    private void PlayCursorSoundFX()
    {
        if (playAlienCursorNoise)
        {
            RuntimeManager.PlayOneShot("event:/SFX/Alien_Cursor", tileCursor.currentTile.transform.position);
        }
        else
        {
            RuntimeManager.PlayOneShot("event:/SFX/Human_Cursor", tileCursor.currentTile.transform.position);
        }
    }
}
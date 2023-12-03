using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HologramDisplayHelper : MonoBehaviour
{
    private Material material;

    public float _RandomGlitchAmount_Speed = 1;
    public float _RandomGlitchConstant_Speed = 1;

    private float _RandomGlitchAmount_Max;
    private float _RandomGlitchAmount_Min;
    private float _RandomGlitchConstant_Max;
    private float _RandomGlitchConstant_Min;

    private float _RandomGlitchAmount_Temp;
    private float _RandomGlitchConstant_Temp;

    private bool _RandomGlitchAmount_Over;
    private bool _RandomGlitchConstant_Over;

    private void OnEnable()
    {
        _RandomGlitchAmount_Over = false;
        _RandomGlitchConstant_Over = false;
        material = GetComponent<Renderer>().material;
        if (material != null)
        {
            _RandomGlitchAmount_Temp = _RandomGlitchAmount_Max = 1;
            _RandomGlitchConstant_Temp = _RandomGlitchConstant_Max = 1;
            _RandomGlitchAmount_Min = material.GetFloat("_RandomGlitchAmount");
            _RandomGlitchConstant_Min = material.GetFloat("_RandomGlitchConstant");
        }
    }

    private void Update()
    {
        if (_RandomGlitchConstant_Over && _RandomGlitchAmount_Over)
        {
            return;
        }

        _RandomGlitchAmount_Temp = _RandomGlitchAmount_Temp - Time.deltaTime * _RandomGlitchAmount_Speed;
        if (_RandomGlitchAmount_Temp <= _RandomGlitchAmount_Min && !_RandomGlitchAmount_Over)
        {
            _RandomGlitchAmount_Temp = _RandomGlitchAmount_Min;
            _RandomGlitchAmount_Over = true;
            material.SetFloat("_RandomGlitchAmount", _RandomGlitchAmount_Temp);
        }

        _RandomGlitchConstant_Temp = _RandomGlitchConstant_Temp - Time.deltaTime * _RandomGlitchConstant_Speed;
        if (_RandomGlitchConstant_Temp <= _RandomGlitchConstant_Min && !_RandomGlitchConstant_Over)
        {
            _RandomGlitchConstant_Temp = _RandomGlitchConstant_Min;
            _RandomGlitchConstant_Over = true;
            material.SetFloat("_RandomGlitchConstant", _RandomGlitchConstant_Temp);
        }

        if (!_RandomGlitchAmount_Over)
            material.SetFloat("_RandomGlitchAmount", _RandomGlitchAmount_Temp);
        if (!_RandomGlitchConstant_Over)
            material.SetFloat("_RandomGlitchConstant", _RandomGlitchConstant_Temp);
    }
}

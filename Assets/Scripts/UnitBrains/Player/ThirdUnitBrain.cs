using System;
using System.Collections.Generic;
using Model;
using Model.Runtime.Projectiles;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace UnitBrains.Player
{
    public enum ThirdUnitState
    {
        Attack,
        Move
    }
    public class ThirdUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Ironclad Behemoth";
        private float _lastStateChangeTime;
        private float _delayBetweenModes = 1f;
        private const float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;
        private ThirdUnitState _state;
        
        public static int UnitID = 0;
        public int UnitNumber = UnitID++;
        public int MaximumTargets = 3;
        
        private readonly List<Vector2Int> _currentTarget = new();
        
        public ThirdUnitBrain()
        {
            _lastStateChangeTime = Time.time - _delayBetweenModes;
        }
        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            int currentTemperature = GetTemperature();
            
            if (currentTemperature < overheatTemperature)
            {
                for (int i = 0; i < currentTemperature; i++)
                {
                    var projectile = CreateProjectile(forTarget);
                    AddProjectileToList(projectile, intoList);
                    Debug.Log(_temperature + 1);
                }
                IncreaseTemperature();
            }
        }
        
        protected override List<Vector2Int> SelectTargets()
        {
            List<Vector2Int> result = new List<Vector2Int>(); 
            _currentTarget.Clear(); 

            foreach (var target in GetAllTargets())
            {
                _currentTarget.Add(target);
            }

            if (_currentTarget.Count == 0)
            {
                _currentTarget.Add(runtimeModel.RoMap.Bases[IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId]);
            }

            SortByDistanceToOwnBase(_currentTarget);

            int currentUnitNumber = UnitNumber % _currentTarget.Count;
            
            if (_currentTarget.Count > currentUnitNumber && IsTargetInRange(_currentTarget[currentUnitNumber]))
            {
                result.Add(_currentTarget[currentUnitNumber]);
            }

            return result;
        }
        
        private int GetTemperature()
        {
            if(_overheated) return (int) OverheatTemperature;
            else return (int)_temperature;
        }

        private void IncreaseTemperature()
        {
            _temperature += 1f;
            if (_temperature >= OverheatTemperature) _overheated = true;
        }

        public override void Update(float deltaTime, float time)
        {
            ChangeMode();
            base.Update(deltaTime, time);
        }

        private void ChangeMode()
        {
            if (Time.time - _lastStateChangeTime >= _delayBetweenModes)
            {
                _lastStateChangeTime = Time.time;

                if (_state == ThirdUnitState.Attack)
                {
                    _state = ThirdUnitState.Move;
                    GetNextStep();
                }
                else if (_state == ThirdUnitState.Move)
                {
                    _state = ThirdUnitState.Attack;
                    SelectTargets();
                    
                    if (_overheated)
                    {
                        _overheated = false;
                        _temperature = 0f;
                    }
                }
            }
        }
    }
}
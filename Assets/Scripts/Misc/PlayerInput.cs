// GENERATED AUTOMATICALLY FROM 'Assets/Prefabs/PlayerInput.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Steering
{
    public class @PlayerInput : IInputActionCollection, IDisposable
    {
        public InputActionAsset asset { get; }
        public @PlayerInput()
        {
			this.asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerInput"",
    ""maps"": [
        {
            ""name"": ""Player"",
            ""id"": ""09b6aaac-2fea-4b45-bd7d-f750608e4248"",
            ""actions"": [
                {
                    ""name"": ""Fire"",
                    ""type"": ""Button"",
                    ""id"": ""7b8edf87-3052-4cd8-8829-2b973f00929b"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Move"",
                    ""type"": ""Button"",
                    ""id"": ""3533a56b-b80c-46b2-b270-a5bc0b57874f"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""05f6913d-c316-48b2-a6bb-e225f14c7960"",
                    ""path"": ""<Pointer>/press"",
                    ""interactions"": ""Press(behavior=1)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Fire"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""WASD"",
                    ""id"": ""3ae4be08-aca6-43ce-be2b-518b0a4ece99"",
                    ""path"": ""2DVector"",
                    ""interactions"": ""Press(behavior=1)"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""a0e51a75-0528-431d-8ac7-fb7b2519fe49"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""91f933d4-c215-42e0-9171-ac232c2fd96d"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""e640f322-8397-427e-b67c-910f2a9fba81"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""0e76f649-f971-4afc-ab83-5c0053ebaf8c"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""Keyboard&Mouse"",
            ""bindingGroup"": ""Keyboard&Mouse"",
            ""devices"": [
                {
                    ""devicePath"": ""<Keyboard>"",
                    ""isOptional"": false,
                    ""isOR"": false
                },
                {
                    ""devicePath"": ""<Mouse>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        },
        {
            ""name"": ""Gamepad"",
            ""bindingGroup"": ""Gamepad"",
            ""devices"": [
                {
                    ""devicePath"": ""<Gamepad>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        },
        {
            ""name"": ""Touch"",
            ""bindingGroup"": ""Touch"",
            ""devices"": [
                {
                    ""devicePath"": ""<Touchscreen>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        },
        {
            ""name"": ""Joystick"",
            ""bindingGroup"": ""Joystick"",
            ""devices"": [
                {
                    ""devicePath"": ""<Joystick>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        },
        {
            ""name"": ""XR"",
            ""bindingGroup"": ""XR"",
            ""devices"": [
                {
                    ""devicePath"": ""<XRController>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
			// Player
			this.m_Player = this.asset.FindActionMap("Player", throwIfNotFound: true);
			this.m_Player_Fire = this.m_Player.FindAction("Fire", throwIfNotFound: true);
			this.m_Player_Move = this.m_Player.FindAction("Move", throwIfNotFound: true);
        }

        public void Dispose()
        {
            UnityEngine.Object.Destroy( this.asset );
        }

        public InputBinding? bindingMask
        {
            get => this.asset.bindingMask;
            set => this.asset.bindingMask = value;
        }

        public ReadOnlyArray<InputDevice>? devices
        {
            get => this.asset.devices;
            set => this.asset.devices = value;
        }

        public ReadOnlyArray<InputControlScheme> controlSchemes => this.asset.controlSchemes;

        public bool Contains(InputAction action)
        {
            return this.asset.Contains(action);
        }

        public IEnumerator<InputAction> GetEnumerator()
        {
            return this.asset.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void Enable()
        {
			this.asset.Enable();
        }

        public void Disable()
        {
			this.asset.Disable();
        }

        // Player
        private readonly InputActionMap m_Player;
        private IPlayerActions m_PlayerActionsCallbackInterface;
        private readonly InputAction m_Player_Fire;
        private readonly InputAction m_Player_Move;
        public struct PlayerActions
        {
            private @PlayerInput m_Wrapper;
            public PlayerActions(@PlayerInput wrapper) { this.m_Wrapper = wrapper; }
            public InputAction @Fire => this.m_Wrapper.m_Player_Fire;
            public InputAction @Move => this.m_Wrapper.m_Player_Move;
            public InputActionMap Get() { return this.m_Wrapper.m_Player; }
            public void Enable() { this.Get().Enable(); }
            public void Disable() { this.Get().Disable(); }
            public bool enabled => this.Get().enabled;
            public static implicit operator InputActionMap(PlayerActions set) { return set.Get(); }
            public void SetCallbacks(IPlayerActions instance)
            {
                if ( this.m_Wrapper.m_PlayerActionsCallbackInterface != null)
                {
					this.@Fire.started -= this.m_Wrapper.m_PlayerActionsCallbackInterface.OnFire;
					this.@Fire.performed -= this.m_Wrapper.m_PlayerActionsCallbackInterface.OnFire;
					this.@Fire.canceled -= this.m_Wrapper.m_PlayerActionsCallbackInterface.OnFire;
					this.@Move.started -= this.m_Wrapper.m_PlayerActionsCallbackInterface.OnMove;
					this.@Move.performed -= this.m_Wrapper.m_PlayerActionsCallbackInterface.OnMove;
					this.@Move.canceled -= this.m_Wrapper.m_PlayerActionsCallbackInterface.OnMove;
                }
				this.m_Wrapper.m_PlayerActionsCallbackInterface = instance;
                if (instance != null)
                {
					this.@Fire.started += instance.OnFire;
					this.@Fire.performed += instance.OnFire;
					this.@Fire.canceled += instance.OnFire;
					this.@Move.started += instance.OnMove;
					this.@Move.performed += instance.OnMove;
					this.@Move.canceled += instance.OnMove;
                }
            }
        }
        public PlayerActions @Player => new PlayerActions(this);
        private int m_KeyboardMouseSchemeIndex = -1;
        public InputControlScheme KeyboardMouseScheme
        {
            get
            {
                if ( this.m_KeyboardMouseSchemeIndex == -1) this.m_KeyboardMouseSchemeIndex = this.asset.FindControlSchemeIndex("Keyboard&Mouse");
                return this.asset.controlSchemes[this.m_KeyboardMouseSchemeIndex];
            }
        }
        private int m_GamepadSchemeIndex = -1;
        public InputControlScheme GamepadScheme
        {
            get
            {
                if ( this.m_GamepadSchemeIndex == -1) this.m_GamepadSchemeIndex = this.asset.FindControlSchemeIndex("Gamepad");
                return this.asset.controlSchemes[this.m_GamepadSchemeIndex];
            }
        }
        private int m_TouchSchemeIndex = -1;
        public InputControlScheme TouchScheme
        {
            get
            {
                if ( this.m_TouchSchemeIndex == -1) this.m_TouchSchemeIndex = this.asset.FindControlSchemeIndex("Touch");
                return this.asset.controlSchemes[this.m_TouchSchemeIndex];
            }
        }
        private int m_JoystickSchemeIndex = -1;
        public InputControlScheme JoystickScheme
        {
            get
            {
                if ( this.m_JoystickSchemeIndex == -1) this.m_JoystickSchemeIndex = this.asset.FindControlSchemeIndex("Joystick");
                return this.asset.controlSchemes[this.m_JoystickSchemeIndex];
            }
        }
        private int m_XRSchemeIndex = -1;
        public InputControlScheme XRScheme
        {
            get
            {
                if ( this.m_XRSchemeIndex == -1) this.m_XRSchemeIndex = this.asset.FindControlSchemeIndex("XR");
                return this.asset.controlSchemes[this.m_XRSchemeIndex];
            }
        }
        public interface IPlayerActions
        {
            void OnFire(InputAction.CallbackContext context);
            void OnMove(InputAction.CallbackContext context);
        }
    }
}

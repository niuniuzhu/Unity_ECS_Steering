using UnityEngine;

namespace Steering
{
	public class FPSDisplay : MonoBehaviour
	{
		float _deltaTime = 0.0f;
		readonly GUIStyle _style = new GUIStyle();

		void Start()
		{
			DontDestroyOnLoad( this.gameObject );
			this._style.fontSize = Screen.height * 2 / 70;
			this._style.normal.textColor = Color.white;
		}

		void Update() => this._deltaTime += ( Time.unscaledDeltaTime - this._deltaTime ) * 0.1f;

		void OnGUI()
		{
			float msec = this._deltaTime * 1000.0f;
			float fps = 1.0f / this._deltaTime;
			GUILayout.Label( $"{msec:0.0} ms ({fps:0.} fps)", this._style );
		}
	}
}
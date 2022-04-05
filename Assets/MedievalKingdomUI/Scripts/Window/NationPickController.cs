using System;
using System.Collections.Generic;
using MedievalKingdomUI.Scripts.Domain;
using UnityEngine;
using UnityEngine.UI;

namespace MedievalKingdomUI.Scripts.Window
{
    public class NationPickController : MonoBehaviour
    {
        public Text nationNameTextLabel;
        public NationProperties[] nations;

        private int _pickedCharacter;

        private void Start()
        {
            if (nations.Length < 1)
            {
                gameObject.SetActive(false);
            }
            _pickedCharacter = 0;
        }

        public void Next()
        {
            ChangeNationInternal(1);
        }

        public void Previous()
        {
            ChangeNationInternal(-1);
        }

        public void ChangeNation(int index)
        {
            if (index < 0)
            {
                throw new ArgumentException();
            }
            ChangeNationInternal(index - _pickedCharacter);
        }

        private void ChangeNationInternal(int step)
        {
            nations[_pickedCharacter].character.SetActive(false);
            nations[_pickedCharacter].descriptionEmblem.SetActive(false);
            _pickedCharacter = _pickedCharacter + step;
            if (_pickedCharacter >= nations.Length)
            {
                _pickedCharacter = 0;
            } else if (_pickedCharacter < 0)
            {
                _pickedCharacter = nations.Length - 1;
            }

            nations[_pickedCharacter].character.SetActive(true);
            nations[_pickedCharacter].descriptionEmblem.SetActive(true);
            nationNameTextLabel.text = nations[_pickedCharacter].name;
        }
    }
}

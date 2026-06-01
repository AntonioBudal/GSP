// Assets/Scripts/Unity/UI/UI_SaveSlotController.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Corvus.Core.SaveSystem;

public class UI_SaveSlotController : MonoBehaviour
{
    [Header("Elementos de Texto")]
    [SerializeField] private TextMeshProUGUI _txtDayHighlight;
    [SerializeField] private TextMeshProUGUI _txtStatus;
    [SerializeField] private TextMeshProUGUI _txtDate;

    [Header("Interação")]
    [SerializeField] private Button _btnSelect;
    [SerializeField] private Button _btnDelete;
    [SerializeField] private TextMeshProUGUI _txtDeleteButtonLabel;

    private int _slotId;
    private bool _isEmpty;
    private bool _isConfirmingDelete;

    private Action<int, bool> _onSlotSelectedCallback;
    private Action<int> _onSlotDeletedCallback;

    public void Setup(int slotId, SaveSlotMetadata metadata, Action<int, bool> onSlotSelected, Action<int> onSlotDeleted)
    {
        _slotId = slotId;
        _onSlotSelectedCallback = onSlotSelected;
        _onSlotDeletedCallback = onSlotDeleted;
        _isConfirmingDelete = false;

        _btnSelect.onClick.RemoveAllListeners();
        _btnSelect.onClick.AddListener(HandleSelectClicked);

        _btnDelete.onClick.RemoveAllListeners();
        _btnDelete.onClick.AddListener(HandleDeleteClicked);

        if (_btnDelete != null)
        {
            _btnDelete.gameObject.SetActive(metadata != null);
        }

        if (metadata == null)
        {
            // Estado do Slot Vazio
            _isEmpty = true;
            _txtDayHighlight.text = "---";
            _txtStatus.text = "O limiar aguarda...";
            _txtDate.text = $"Registo Vazio [Slot {slotId}]";
            if (_txtDeleteButtonLabel != null) _txtDeleteButtonLabel.text = "X";
        }
        else
        {
            // Estado do Slot Ocupado
            _isEmpty = false;
            _txtDayHighlight.text = $"DIA {metadata.CurrentDay}";
            _txtStatus.text = metadata.NarrativeStatus;
            _txtDate.text = $"Última vigília: {metadata.LastPlayedDate:dd/MM/yyyy HH:mm}";
            if (_txtDeleteButtonLabel != null) _txtDeleteButtonLabel.text = "X";
        }
    }

    private void HandleSelectClicked()
    {
        // Se o jogador estava prestes a apagar e clicou no slot, cancela a ação de apagar
        if (_isConfirmingDelete)
        {
            ResetDeleteState();
            return;
        }

        _onSlotSelectedCallback?.Invoke(_slotId, _isEmpty);
    }

    private void HandleDeleteClicked()
    {
        if (_isEmpty) return;

        if (!_isConfirmingDelete)
        {
            // Primeiro clique: pede confirmação
            _isConfirmingDelete = true;
            _txtDeleteButtonLabel.text = "CONFIRMAR?";
            _txtDeleteButtonLabel.color = Color.red;
        }
        else
        {
            // Segundo clique: executa a eliminação
            _onSlotDeletedCallback?.Invoke(_slotId);
        }
    }

    public void ResetDeleteState()
    {
        _isConfirmingDelete = false;
        if (_txtDeleteButtonLabel != null)
        {
            _txtDeleteButtonLabel.text = "X";
            _txtDeleteButtonLabel.color = Color.white;
        }
    }
}
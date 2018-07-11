using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class HandControl : MonoBehaviour {
	
	[SerializeField]
	public GameObject handCardPrefab;

	[SerializeField]
	public GameObject Deck;

/* 
	public IList<CardControl> Cards
	{
		get;
		set;
	}
*/	
	public List<CardControl> Cards = new List<CardControl>();

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void addCard ()
	{
		GameObject cardobj = (GameObject)Instantiate (
				handCardPrefab,
				Deck.transform.position,
				Quaternion.identity
			);
		cardobj.transform.parent = transform;
		CardControl card = cardobj.GetComponent<CardControl>();
		card.handControl = this;
		card.initPosition = getPosition();
		Cards.Add(card);
	}

	public Vector3 getPosition()
	{
		int x = 7 * Cards.Count;
		return new Vector3(x, 0 , 0);
	}

	public void addClickObserver(CardControl _card)
	{
		_card.OnClickAsObservabale.SelectMany(_ => _card.SelectedAnimation())
			.Subscribe(_ =>
			{
				Debug.Log("selected");
				resetSelected(_card);
			});
	}

	public void addDealObserver(CardControl _card)
	{
        _card.FlipAnimation()
            .Subscribe(_ =>
            {
                Debug.Log("Deal Animation End");
            });		
	}

	/*
	選択したカード以外を非選択状態にする。
	*/
	private void resetSelected (CardControl selectedCard)
	{
		foreach (CardControl _card in Cards)
		{
			Debug.Log(selectedCard.isSelected);
			if (selectedCard != _card)
			{
				_card.isSelected = false;
			}
		}
	}
}

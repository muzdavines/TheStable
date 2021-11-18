function OnTriggerEnter () {

gameObject.GetComponent(Animation).Play("Open");

}

function OnTriggerExit () {

gameObject.GetComponent(Animation).Play("Close");

}
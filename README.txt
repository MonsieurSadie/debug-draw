Simple functions for being to able to draw debugmeshes in update functions
eg : 
void Update()
{
  Draw.color = Color.blue;
  Draw.WireCircle(transform.position, 3.2f);
}
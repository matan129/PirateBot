package bots;

import java.util.*;

import pirates.game.*;

public class Sailing {
	private PirateGame game=MyBot.game;
	
	public Sailing() {
		
	}
	public void simpleSail(Navy myNavy) {
		Direction dir=Direction.NORTH;		
		if(myNavy.getMainTarget()!=null) {
			for(int pirateID : myNavy.getShiplist()) {
				Pirate myPirate=game.getMyPirate(pirateID);
				List<Direction> possibleDirections=game.getDirections(myPirate.getLocation(), myNavy.getMainTarget().getLocation());
				for(int i=0; i<possibleDirections.size(); i++) {
					dir=possibleDirections.get(i);
					if(checkIfPassable(myPirate, dir)) {
						break;
					}
				}
				game.setSail(myPirate, dir);				
			}
		}
	}
	public void setSail (Navy myNavy, String chooseTargetMode) 
	{MyBot.game.debug("setSail");
		if(myNavy.getMode()==NavyMode.CONQUERING) {
			this.whileCapturing(myNavy);
			return;
		}
		if(myNavy.getMode()==NavyMode.WAITING) {
			this.whileWaiting(myNavy);
			if(myNavy.getMode()==NavyMode.WAITING)
				return;
		}
		if(myNavy.getIsCloaked()) {
			Direction dir=this.setDirectionWhileGhost(myNavy);
			if(dir!=null) {
				for(int eldar:myNavy.getShiplist()){
					Pirate pirate=MyBot.game.getMyPirate(eldar);
					MyBot.game.setSail(pirate, dir);
				}
			}
			else {
				game.debug("dir=null...");
			}
			return;
		}
		if(myNavy.getMode()==NavyMode.SAILING) {
			Direction dir=this.setDirection(myNavy,chooseTargetMode);
			MyBot.game.debug("%s",dir);
			if(dir!=null) {				
				for(int id:myNavy.getShiplist()) {
					Pirate eldar=MyBot.game.getMyPirate(id);
					game.setSail(eldar, dir);
				}
			}
			else {
				game.debug("dir=null...");
			}
		}
	}
	public Direction setDirectionWhileGhost(Navy myNavy) {
		List<Direction> possibleDirections;
		int max=-1000;
		Direction bestDir=Direction.NOTHING;
		Island isle=MyBot.game.getIsland(myNavy.getMainTarget().getID());
		if(myNavy.getMode()==NavyMode.WAITING&&myNavy.getLocation()==myNavy.getMainTarget().getLocation()) {
			possibleDirections=MyBot.dirList;
			possibleDirections.remove(Direction.NOTHING);
		}
		else {
			possibleDirections=MyBot.game.getDirections(myNavy.getLocation(), myNavy.getMainTarget().getLocation());
		}
		if(myNavy.isSafeToReveal()&&MyBot.game.InRange(myNavy.getLocation(), myNavy.getMainTarget().getLocation())) {
			MyBot.game.debug("reveal");
			return Direction.REVEAL;
		}
		for(Direction dir: MyBot.dirList) {
			int grade=0;
			Location destination=MyBot.game.destination(myNavy.getLocation(), dir);
			if(MyBot.game.isOccupied(destination)||!MyBot.game.isPassable(destination)) {
				continue;
			}
			if(possibleDirections.contains(dir)) {
				grade++;
			}
			if(grade>max) {
				bestDir=dir;
				max=grade;
			}
		}
		return bestDir;
	}
	public void whileCapturing(Navy myNavy)
	{MyBot.game.debug("whileCapturing");
		EnemyNavy enavy=null;
		int min=1000000000;
		for(EnemyNavy enemy:Enemy.enemyNavyList)
		{
			if(enemy.getSize()==0||enemy.getTarget()==-1)
				continue;
			if(MyBot.game.distance(enemy.getLoc(), myNavy.getLocation())<min&&enemy.getTarget()==myNavy.getMainTarget().getID())
			{
				min=MyBot.game.distance(enemy.getLoc(), myNavy.getLocation());
				enavy=enemy;
			}
		}
		if(enavy!=null&&enavy.getSize()>0)
		{
			MyBot.game.debug("enavy!=null");
			MyBot.game.debug("danger while capturing %s",enavy.getLoc());
			double distance=enavy.turnsToReachTarget()-Math.sqrt(game.getAttackRadius())-1;
			int turnsUntilCapture=MyBot.game.getIsland(enavy.getTarget()).getCaptureTurns()-MyBot.game.getIsland(enavy.getTarget()).getTurnsBeingCaptured();
			if(MyBot.game.getIsland(enavy.getTarget()).getOwner()==Constants.ENEMY)
			{
				turnsUntilCapture+=MyBot.game.getIsland(enavy.getTarget()).getCaptureTurns();
			}
			MyBot.game.debug("attack radius:%s, sqrt(attack radius): %s",game.getAttackRadius(),Math.sqrt(game.getAttackRadius()));
			MyBot.game.debug("turns to capture: %s, distance: %s",turnsUntilCapture, distance);
			if(distance<=turnsUntilCapture&&MyBot.distanceSquare(myNavy.getMainTarget().getLocation(), enavy.getLoc())<=17)//TODO:IF THEY CHANGE THE RANGE IN THE FINALS, CHANGE THIS
			{MyBot.game.debug("need to move");
				/*if(enavy.getSize()==1&&MyBot.game.canCloak()&&myNavy.isGhostable())//if we can cloak the navy
				{
					MyBot.game.cloak(MyBot.game.getMyPirate(myNavy.getShiplist().get(0)));
					myNavy.setMode(NavyMode.WAITING);
					return;//cloaks the navy
				}*/
				if(enavy.getSize()<myNavy.getSize())
				{
					return;
				}
				if(enavy.getSize()>=myNavy.getSize())
				{
					for(int eldarid:myNavy.getShiplist())
					{
						Pirate eldar=MyBot.game.getMyPirate(eldarid);
						Direction dir=MyBot.game.getDirections(enavy.getLoc(), myNavy.getLocation()).get(0);
						if(dir!=null) {
							if(this.checkIfPassable(eldar, dir)) {
								MyBot.game.setSail(eldar, dir);
								continue;
							}
							else {
								dir=MyBot.game.getDirections(myNavy.getLocation(),enavy.getLoc()).get(0);
								if(dir!=null) {
									if(this.checkIfPassable(eldar, dir)) {
										MyBot.game.setSail(eldar, dir);
										continue;
									}
									else
										continue;
								}
							}
						}
						else {
							game.debug("dir=null...");
						}
					}
					myNavy.setMode(NavyMode.WAITING);
					return;
				}
			}
		}
	}
	public void whileWaiting(Navy myNavy)
	{MyBot.game.debug("whileWaiting");
		EnemyNavy enavy=null;
		int min=1000000000;
		for(EnemyNavy enemy:Enemy.enemyNavyList)
		{
			if(enemy.getTarget()==myNavy.getMainTarget().getID())
			{if(enemy.getSize()==0)
				continue;
				if(MyBot.game.distance(enemy.getLoc(), myNavy.getLocation())<min){
					min=MyBot.game.distance(enemy.getLoc(), myNavy.getLocation());
					enavy=enemy;
				}
			}
		}
		if(enavy==null||enavy.getSize()==0)
		{
			myNavy.setMode(NavyMode.SAILING);
			return;
		}
		Island isle=MyBot.game.getIsland(myNavy.getMainTarget().getID());
		if(isle.getTeamCapturing()!=Constants.ENEMY&&isle.getOwner()!=Constants.ME) {
			//if the target isnt being captured by enemy
			if(MyBot.distanceSquare(isle.getLocation(), myNavy.getLocation())<11) {
				//if the navy is still in the range of the target
				Direction dir= this.setDirection(myNavy, TargetUtilities.MAIN_TARGET);
				if(dir!=null) {
					for(int eldarid:myNavy.getShiplist()){
						Pirate eldar=MyBot.game.getMyPirate(eldarid);
						MyBot.game.setSail(eldar, dir);
					}
				}
				else {
					game.debug("dir=null...");
				}
			}
			else {
				myNavy.setMode(NavyMode.SAILING);
				return;
			}
		}
		else{
			myNavy.setMode(NavyMode.SAILING);
		}
	}
	public EnemyNavy enemyNavyAimingAtIsland(Navy myNavy){
		EnemyNavy enavy=null;
		int min=1000000000;
		for(EnemyNavy enemy:Enemy.enemyNavyList){
			if(enemy.getTarget()==myNavy.getMainTarget().getID())
			{if(enemy.getSize()==0)
				continue;
				if(MyBot.game.distance(enemy.getLoc(), myNavy.getLocation())<min){
					min=MyBot.game.distance(enemy.getLoc(), myNavy.getLocation());
					enavy=enemy;
				}
			}
		}
		return enavy;
	}
	public Direction setDirection(Navy myNavy, String chooseTargetMode)
	{//MyBot.game.debug("setDirection");
		int max=-1000;
		List<Direction> possibleDirections;
//		if(myNavy.isGhostable()&&MyBot.game.canCloak())
//			return Direction.CLOAK;
		if(myNavy.getMode()==NavyMode.WAITING) {
			possibleDirections=game.getDirections(myNavy.getMainTarget().getLocation(),myNavy.getLocation());
		}
		possibleDirections=game.getDirections(myNavy.getLocation(),myNavy.getMainTarget().getLocation());
		if(chooseTargetMode.equals(TargetUtilities.MAIN_TARGET)) {
			possibleDirections=game.getDirections(myNavy.getLocation(),myNavy.getMainTarget().getLocation());
		}
		if (chooseTargetMode.equals(TargetUtilities.SECONDARY_TARGET)) {
			possibleDirections=game.getDirections(myNavy.getLocation(),myNavy.getSecondaryTarget().getLocation());
		}
		//List<Direction> wrongDirection=game.getDirections(myNavy.getTarget().getLocation(), myNavy.getLocation());
		Pirate eldar=MyBot.game.getMyPirate(myNavy.getShiplist().get(0));
		List<Direction> bestDirs=new ArrayList<Direction>();
		for(Direction dir:MyBot.dirList){
			Location destination=MyBot.game.destination(eldar.getLocation(), dir);
			int grade=0;
		//	if(MyBot.game.isOccupied(destination))
		//	{
		//		continue;
		//	}
			if(MyBot.game.getMyCloaked()!=null) {
				if(destination==MyBot.game.getMyCloaked().getLocation()) {
					grade=grade-3;
				}
			}
			if(!MyBot.game.isPassable(destination)) {
				continue;
			}
			int danger=myNavy.InDangerInNextStep(eldar.getId(), dir);
			if(danger>0) {
				continue;
			}
			if(danger==0) {
				grade=grade-3;
			}
//			if(danger<0)
//				grade++;
			if(possibleDirections.contains(dir)) {
				grade=grade+2;
			}
			//if(wrongDirection.contains(dir)||dir==Direction.NOTHING)
				//grade--;
			if(grade==max) {
				bestDirs.add(dir);
			}
			if(grade==2) {
				return dir;
			}
			if(grade>max) {
				bestDirs.clear();
				bestDirs.add(dir);
				max=grade;
			}
			if(myNavy.isGhostable()&&MyBot.game.canCloak()&&max<2) {
				return Direction.CLOAK;
			}
		}
		if(bestDirs!=null && bestDirs.size()>0) {
			return bestDirs.get(0);
		}
		else {
			return null;
		}
		//return this.randomDirection(bestDirs);
	} 
//	public Direction setDirection(Navy myNavy) {
//		int max=-1000;
//		List<Direction> possibleDirections=game.getDirections(myNavy.getLocation(),myNavy.getTarget().getLocation());
//		
//		List<Direction> bestDirs=new ArrayList<Direction>();
//		for(Direction dir:MyBot.dirList) {
//			
//			int grade=0;
//		//	if(MyBot.game.isOccupied(destination))
//		//	{
//		//		continue;
//		//	}
//			if(!myNavy.isOnePossibleInNextStep(dir)) {
//				continue;
//			}
//			if(myNavy.isOneInDangerInNextStep(dir)) {
//			
//				continue;
//			}
//			game.debug("Safe direction: "+dir.toString()+" To First Pirate: "+myNavy.getShiplist().get(0));
//			if(possibleDirections.contains(dir)) {
//				grade++;
//			}
//			if(grade==max) {
//				bestDirs.add(dir);
//			}
//			if(grade>max) {
//				bestDirs.clear();
//				bestDirs.add(dir);
//				max=grade;
//			}
//		}
//		Direction choosen =this.randomDirection(bestDirs); 
//		if(choosen!=null)
//		game.debug("Choosen direction: "+choosen.toString());
//		return choosen;
//	} 
	/*public void setDirection(Navy myNavy) {
		for(Integer pir:myNavy.getShiplist() ) {
			int max=-1000;
			List<Direction> possibleDirections=game.getDirections(myNavy.getLocation(),myNavy.getTarget().getLocation());
			
			Pirate eldar=MyBot.game.getMyPirate(pir);
//			List<Direction> friendlyasist=game.getDirections(eldar,MyBot.game.getMyPirate(myNavy.getShiplist().get(0)));
			List<Direction> bestDirs=new ArrayList<Direction>();
			for(Direction dir:MyBot.dirList) {
				Location destination=MyBot.game.destination(eldar.getLocation(), dir);
				int grade=0;
			//	if(MyBot.game.isOccupied(destination))
			//	{
			//		continue;
			//	}
				if(!MyBot.game.isPassable(destination)) {
					continue;
				}
				if(myNavy.InDangerInNextStep(eldar.getId(), dir)) {
					continue;
					
				}
	//			Boolean collution=false;
	//			for(Pirate pirate : MyBot.game.myPirates())
	//			{
	//				if(pirate.getId()!=eldar.getId()&& pirate.getLocation()==destination)
	//				{
	//					collution=true;
	//				
	//				}
	//				if(collution==true)
	//					break;
	//			}
	//			if(collution==true)
	//				continue;
				if(possibleDirections.contains(dir)) {
					grade++;
					
				}
//				if(friendlyasist.contains(dir))
//					grade++;
				if(grade==max) {
					game.debug("Safe Direction: "+dir.toString()+" To pirate : "+eldar.toString());
					bestDirs.add(dir);
				}
				if(grade>max) {
					bestDirs.clear();
					bestDirs.add(dir);
					max=grade;
				}
			}
			Direction choosen = this.randomDirection(bestDirs);
			if(choosen!=null)
			game.debug("Choosen Direction: "+choosen.toString()+" To pirate : "+eldar.toString());
			if(choosen==null)
				game.setSail(eldar, Direction.NOTHING);
			else
				game.setSail(eldar, choosen);
		}
	} */
	public Direction randomDirection(List<Direction> directionList) {
		if(directionList.size()==0||directionList==null) {
			return null;
		}
		int i=(int) ((Math.random()*(directionList.size())));  
		return directionList.get(i);
	}
/*	public void setSail (Navy myNavy) {
		Direction dir=Direction.NORTH;		
		if(myNavy.getTarget()!=null) {
			for(int pirateID : myNavy.getShiplist()) 
			{
				Pirate myPirate=game.getMyPirate(pirateID);
				game.setSail(myPirate, getBestDirection(myNavy,myPirate));				
			}
		}
	}
	public Direction getBestDirection(Navy myNavy,Pirate myPirate)
	{Direction dir=Direction.NORTH;	
		List<Direction> possibleDirections=MyBot.game.getDirections(myPirate.getLocation(), myNavy.getTarget().getLocation());
		for(int i=0; i<possibleDirections.size(); i++) 
		{
			dir=possibleDirections.get(i);
			if(checkIfPassable(myPirate, dir)&&!myNavy.InDangerInNextStep(myPirate.getId(),dir)) {
				return dir;
			}
		}
		List<Direction> possibleDirections1=game.getDirections(myNavy.getTarget().getLocation(),myPirate.getLocation());
		for(int i=0; i<possibleDirections1.size(); i++) 
		{
			dir=possibleDirections1.get(i);
			if(checkIfPassable(myPirate, dir)) {
				return dir;
			}
		}
		return dir;
		
	}*/
	public boolean isTargetTaken (Target tr) {
		List<Navy> naviesList=Manager.naviesList;
		for(int i=0; i<naviesList.size(); i++) {
			if(naviesList.get(i).getMainTarget()!=null) {
				if(naviesList.get(i).getMainTarget().getType().equals(tr.getType()) && naviesList.get(i).getMainTarget().getID()==tr.getID()) {
					return true;
				}
			}
		}
		return false;
	}
	public boolean isCurrentTarget(Target tr, Navy myNavy) {
		if(myNavy.getMainTarget()==null) {
			return false;
		}
		if(myNavy.getMainTarget().getType()==tr.getType() && myNavy.getMainTarget().getID()==tr.getID()) {
			return true;
		}
		return false;
	}
	
	public boolean checkIfPassable(Pirate myPirate, Direction dir) {
		Location currentLocation=myPirate.getLocation();
		Location locationToCheck=myPirate.getLocation();
		if(dir.equals(Direction.EAST)) {
			locationToCheck=new Location(currentLocation.row, currentLocation.col+1);
		}
		if(dir.equals(Direction.WEST)) {
			locationToCheck=new Location(currentLocation.row, currentLocation.col-1);
		}
		if(dir.equals(Direction.NORTH)) {
			locationToCheck=new Location(currentLocation.row-1, currentLocation.col);
		}
		if(dir.equals(Direction.SOUTH)) {
			locationToCheck=new Location(currentLocation.row+1, currentLocation.col);
		}
		game.debug("Pirate "+myPirate.getId()+" : Location to check - "+locationToCheck.toString());
		return game.isPassable(locationToCheck)&&!MyBot.game.isOccupied(locationToCheck);
	}
	/*public void formUp(Navy myNavy)
	{
		game.debug("Forming");
		Pirate point=game.getMyPirate(myNavy.getShiplist().get(0));
	for(Integer pirate: myNavy.getShiplist())
	{
		
			game.setSail(game.getMyPirate(pirate), game.getDirections(point.getLocation(),game.getMyPirate(pirate).getLocation()).get(0));
	}
	
	}
	public void formUpWhileConquering(Navy myNavy)
	{
		Location mark=new Location(0,0);
		for(Integer eldar: myNavy.getShiplist())
		{
			if(MyBot.game.isCapturing(MyBot.game.getMyPirate(eldar)))
			{
				mark=MyBot.game.getMyPirate(eldar).getLocation();
				break;
			}
					
		}
		for(Integer eldar: myNavy.getShiplist())
		{
		game.setSail(MyBot.game.getMyPirate(eldar), MyBot.game.getDirections(MyBot.game.getMyPirate(eldar).getLocation(), mark).get(0));
		}
		
	}*/
	/*public void setSail (Navy myNavy)  {
	if(myNavy.getMode()==NavyMode.CONQUERING) {
		this.whileCapturing(myNavy);
		this.formUpWhileConquering(myNavy);
		return;
	}
	if(myNavy.getMode()==NavyMode.WAITING) {
		this.whileWaiting(myNavy);
		if(myNavy.getMode()==NavyMode.WAITING) {
			return;
		}
	}
	if(myNavy.getMode()==NavyMode.SAILING) {
		if(MyBot.game.getTurn()!=0){
//		Direction dir=this.setDirection(myNavy);
//		if(dir==null) {
//			return;
//		}
		this.setDirection(myNavy);
		}
		else
			myNavy.setMode(NavyMode.FORMING);
			
	}
	if(myNavy.getMode()==NavyMode.FORMING)
	{
		int count=0;
		if(count!=5)
		{
			count++;
			formUp(myNavy);
		}
		else
		{
			count=0;
			myNavy.setMode(NavyMode.SAILING);
		}
	}
	*/
/*	Direction dir=Direction.NORTH;		
	if(myNavy.getTarget()!=null) {
		for(int pirateID : myNavy.getShiplist()) {
			if(myNavy.InDangerInNextStep(pirateID))
			{
				Pirate myPirate=game.getMyPirate(pirateID);
				List<Direction> possibleDirections=game.getDirections(myNavy.getTarget().getLocation(),myPirate.getLocation());
				for(int i=0; i<possibleDirections.size(); i++) {
					dir=possibleDirections.get(i);
					if(checkIfPassable(myPirate, dir)) {
						break;
					}
				}
				game.setSail(myPirate, dir);
				continue;
			}
			Pirate myPirate=game.getMyPirate(pirateID);
			List<Direction> possibleDirections=game.getDirections(myPirate.getLocation(), myNavy.getTarget().getLocation());
			for(int i=0; i<possibleDirections.size(); i++) {
				dir=possibleDirections.get(i);
				if(checkIfPassable(myPirate, dir)) {
					break;
				}
			}
		 	game.setSail(myPirate, dir);				
		}
	} */

}

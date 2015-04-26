package bots;

import java.util.*;

import pirates.game.*;

public class Navy {
	
	private List<Integer>  shipsList;
    private Target mainTarget=null;
    private Target secondaryTarget=null;
    private PirateGame game=MyBot.game;
    public Navigator myNav;
    private int id;
    private NavyMode mode;
    private boolean isCloaked=false;;
    
    public Navy() {
    	this.id=Manager.naviesList.size(); //TODO : Check if this is alright
    	myNav=new Navigator(this);
        this.game.debug("navyconst");
        this.shipsList = new ArrayList<Integer>();
        this.mode=NavyMode.INITIAL;
    }
    public Navy(int id) {
    	this.id=id;
    	myNav=new Navigator(this);
        this.game.debug("navyconst");
        this.shipsList = new ArrayList<Integer>();
        this.mode=NavyMode.INITIAL;
    }
    public void setIsCloaked(boolean cloaked) {
    	this.isCloaked=cloaked;
    }
    public boolean getIsCloaked() {
    	return this.isCloaked;
    }    
    public void setMode(NavyMode mode) {
    	this.mode=mode;
    }
    public NavyMode getMode() {
    	return this.mode;
    }
    public void removeShip(Pirate eldar) {
    	int id=eldar.getId();
    	int x= this.shipsList.indexOf(id);
    	if(x==-1) {
    		return;
    	}
    	this.shipsList.remove(x);
    }
    public int getID() {
    	return this.id;
    }
    public void setID(int id) {
    	this.id=id;
    }
    public void setGame(PirateGame game) {
        this.game = game;
    }
    public List<Integer> getShiplist() {
    	//returns the ships in the list
       return this.shipsList;

    }
    public Target getMainTarget() {
        return this.mainTarget;
    }
    public void setMainTarget(Target tr) {
    	this.mainTarget = tr;
    }
    public Target getSecondaryTarget() {
    	return this.secondaryTarget;
    }
    public void setSecondaryTarget(Target tr) {
    	this.secondaryTarget = tr;
    }
    public Location getLocation() {
    	// returns the navy's location
        if (this.shipsList!=null || this.shipsList.size()>0) {
        	for(int myPirateID : this.shipsList) {
        		Pirate myPirate=game.getMyPirate(myPirateID);
        		if(!myPirate.isLost()) {
        			return myPirate.getLocation();
        		}
        	}
        }
        return null;
    }
    public boolean isBusy() {
    	if (this.mainTarget == null) {
            return false;
    	}
    	// returns whether the navy is busy or not
		if(this.mainTarget.getType().equals(TargetUtilities.ISLAND)) {
			Island isle=game.getIsland(this.mainTarget.getID());
			if(isle.getOwner()!=Constants.ME) {
				return true;
			}
		}
		if(this.mainTarget.getType().equals(TargetUtilities.ENEMY_PIRATE)) {
			Pirate pir=game.getEnemyPirate(this.mainTarget.getID());
			if(!pir.isLost() || !pir.isCloaked()) {
				return true;
			}
		}
        return false;
    }
    public void addShip(int Eldar) {
    	// adds a ship to the navy
        this.shipsList.add(Eldar);
    }
    public void addShip(Pirate Eldar) {
    	// adds a ship to the navy
    	int eldarid=Eldar.getId();
        this.shipsList.add(eldarid);
    }
    public int getSize()  {
    	if(this.shipsList!=null){
    		return this.shipsList.size();
    	}
    	else {
    		return 0;
    	}
    }
//    public List<Pirate> EnemyFriendlyInRange(Integer id)
//	{
//     Pirate theEnemy= MyBot.game.getEnemyPirate(id);
//     List<Pirate> Help = new ArrayList<Pirate>();
//     
//     for(Pirate friend:MyBot.game.enemyPirates())
//     {
//    	 if(theEnemy.compareTo(friend)!=0 && MyBot.inRange(friend.getLocation(), theEnemy.getLocation()))
//    	 {
//    		 Help.add(friend);
//    	 }
//     }
//     return Help;
//	}
    public int InDangerInNextStep(int myPirate1,Direction dir) {
    	// changed a bit
    	Pirate myPirate = this.game.getMyPirate(myPirate1);
        HashSet<Pirate> myPiratesInRange = new HashSet<Pirate>();
        HashSet<Pirate> enemyPiratesInRange = new HashSet<Pirate>();
        //add supportive pirates to the list
        Location loc = this.game.destination(myPirate, dir);
        for(Pirate erick: MyBot.game.enemyPirates())
        {
        	if(MyBot.isOnIsland(erick.getLocation()))
        		continue;
        for(Direction enemyDir:this.game.getDirections(erick.getLocation(),myPirate.getLocation()))
        	{
        		Location enemyLoc=MyBot.game.destination(erick.getLocation(), enemyDir);
        		if(!MyBot.isOnIsland(enemyLoc)&&MyBot.inRange(loc,enemyLoc))
        		{
        			enemyPiratesInRange.add(MyBot.game.getEnemyPirate(erick.getId()));
        		}
        	}
        }
        if(enemyPiratesInRange.size()==0)
        	return -1;
        for(Navy navy:Manager.naviesList)
        {
        	if(navy.getShiplist().size()==0||navy.getMainTarget()==null||navy.getMainTarget().getID()==-1)
        		continue;
        	for(int pirate:navy.shipsList)
        	{
        		Pirate eldar=MyBot.game.getMyPirate(pirate);
        		Direction dirFriend= MyBot.game.getDirections(eldar, navy.getMainTarget().getLocation()).get(0);
        		Location friendLoc=MyBot.game.destination(eldar, dirFriend);
        		if(!MyBot.isOnIsland(friendLoc)&&MyBot.inRange(loc, friendLoc)&&!eldar.isCloaked()){
        			myPiratesInRange.add(eldar);
        		}
        	}
        }
        return enemyPiratesInRange.size() - myPiratesInRange.size();
    }
    public boolean isPirateOnTarget(Pirate myPirate) {
        if (myPirate.getLocation() == this.mainTarget.getLocation()) {
            return true;
        }
        return false;
    }
  /*  public boolean InDangerInNextStep(int myPirate1,Direction dir) {
    	// changed a bit
    	Pirate myPirate = this.game.getMyPirate(myPirate1);
        HashSet<Pirate> myPiratesInRange = new HashSet<Pirate>();
        HashSet<Pirate> enemyPiratesInRange = new HashSet<Pirate>();
        //add supportive pirates to the lists
        Location loc = this.game.destination(myPirate, dir);
        for (Pirate pirate : game.myPirates()) {
            if (this.game.inRange(pirate, loc)) {
            	if(!MyBot.game.isCapturing(pirate))
            	{
            		myPiratesInRange.add(pirate);
            	}
            }
        }
        //add potential enemies to the list
        for (Pirate enemy : game.enemyPirates()) {
            for (Direction direction : this.game.getDirections(enemy, myPirate)) {
                if (this.game.InRange(loc, this.game.destination(enemy, direction))) {
                	if(!MyBot.game.isCapturing(enemy))
                	{
                		enemyPiratesInRange.add(enemy);
                	}
                }
            }
        }
        return myPiratesInRange.size() < enemyPiratesInRange.size();
    } */
    public boolean isSafeToReveal() {
    	Pirate myPirate = this.game.getMyPirate(this.shipsList.get(0));
        HashSet<Pirate> myPiratesInRange = new HashSet<Pirate>();
        HashSet<Pirate> enemyPiratesInRange = new HashSet<Pirate>();
        //add supportive pirates to the lists
        Location loc =myPirate.getLocation();for(Pirate erick: MyBot.game.enemyPirates())
        {
        	if(MyBot.isOnIsland(erick.getLocation()))
        		continue;
        for(Direction enemyDir:this.game.getDirections(erick.getLocation(),myPirate.getLocation()))
        	{
        		Location enemyLoc=MyBot.game.destination(erick.getLocation(), enemyDir);
        		if(!MyBot.isOnIsland(enemyLoc)&&MyBot.inRange(loc,enemyLoc))
        		{
        			enemyPiratesInRange.add(MyBot.game.getEnemyPirate(erick.getId()));
        		}
        	}
        }
        if(enemyPiratesInRange.size()==0)
        	return true;
        for(Navy navy:Manager.naviesList)
        {
        	if(navy.getShiplist().size()==0||navy.getMainTarget()==null||navy.getMainTarget().getID()==-1)
        		continue;
        	for(int pirate:navy.shipsList)
        	{
        		Pirate eldar=MyBot.game.getMyPirate(pirate);
        		Direction dirFriend= MyBot.game.getDirections(eldar, navy.getMainTarget().getLocation()).get(0);
        		Location friendLoc=MyBot.game.destination(eldar, dirFriend);
        		if(!MyBot.isOnIsland(friendLoc)&&MyBot.inRange(loc, friendLoc)&&!eldar.isCloaked()){
        			myPiratesInRange.add(eldar);
        		}
        	}
        }
        return enemyPiratesInRange.size() < myPiratesInRange.size();
        	
    }
    
    public void deleteLostPirates()
    {
    	for(Pirate eldar:MyBot.game.myLostPirates())// deletes a lost pirate from its navy
		{
			this.removeShip(eldar);
			MyBot.game.debug("delete lost pirate");
		}
    }
    public boolean isGhostable() {
		if(this.getSize()!=1) {
			return false;
		}
		if(!(this.mode==NavyMode.SAILING||this.mode==NavyMode.INITIAL||this.mode==NavyMode.WAITING||this.mode==NavyMode.CONQUERING)) {
			return false;
		}
		MyBot.game.debug("ghostable");
		return true;
	}
	public boolean isGhosted() {
		for(int id:this.shipsList) {
			Pirate eldar=MyBot.game.getMyPirate(id);
			if(eldar.isCloaked()) {
				return true;
			}
		}
		return false;
	}
    
    //TODO: reGroup func
   /* public void reGroup()
    {
        Location loc;

    } */ 
    public boolean isGrouped() {
    // checks if all the pirates are in range
    	for (int Eldar1 : this.shipsList) {
            Pirate Eldar = game.getMyPirate(Eldar1);
            int count = 0;
            for (int pirate1 : this.shipsList) {
                Pirate pirate = game.getMyPirate(pirate1);
                if (this.game.inRange(Eldar, pirate))
                    count++;
            }
            if (count != this.getSize()) {
                return false;
            }
        }
        return true;
    }
    public boolean isAllDead() {
        if(this.shipsList==null || this.shipsList.size()==0) {
        	return true;
        }
    	for (int indPirate : this.shipsList) {
            Pirate pirate = this.game.getMyPirate(indPirate);
            if (pirate.isLost() == false) {
                return false;
            }
        }
        return true;
    }
    public boolean isCapturing() {    	
		if(this.mainTarget.getType().equals(TargetUtilities.ISLAND)) {
			Island isle=game.getIsland(this.mainTarget.getID());
			if(isle.getTeamCapturing()==Constants.ME && this.isOnTarget()) {
				return true;
			}
		}
       return false;       
    }
    public boolean isOnTarget() {
        for (int pirateid : this.shipsList) {
            Pirate eldar = this.game.getMyPirate(pirateid);
            if (MyBot.game.distance(eldar.getLocation(),this.mainTarget.getLocation())<=1) {
                return true;
            }
        }
        return false;
    }
    public boolean isOnePossibleInNextStep(Direction dir) {
        for(int pirate1 : this.shipsList) {
            if (!MyBot.game.isPassable(game.destination(game.getMyPirate(pirate1).getLocation(), dir))) {
                return false;
            }
        }
        return true;        
    }
    public Pirate closestPirateToTarget(Location loc)//returns the closest pirate to the target from the navy.
    {
    	if(this.shipsList.size()==0)
    		return null;
    	if(loc==null)// if the loc is null, it returns the last pirate in the list
    		return MyBot.game.getMyPirate(this.getShiplist().get(this.getShiplist().size()-1));
    	int mindis=10000000;
    	Pirate MinDis=null;
    	for(int pirateid:this.shipsList)
    	{
    		Pirate Eldar=MyBot.game.getMyPirate(pirateid);
    		if(MyBot.game.distance(Eldar.getLocation(), loc)<mindis)
    		{
    			mindis=MyBot.game.distance(Eldar.getLocation(), loc);
    			MinDis=Eldar;
    		}
    	}
    	return MinDis;
    }
    public boolean isEmpty()
    {
    	return this.shipsList.size()==0;
    }
	public int isDividable() {
		// checks if the navy can be divided
		if(this.mainTarget!=null && (this.shipsList!=null && this.shipsList.size()>0)) {
			return this.shipsList.size()-this.mainTarget.getRequirements();
		}
		else {
			return -1;
		}
//		if(this.target==null)
//			return true;
//		if(this.target.getType().equals("Pirate"))
//			return false;
//		if(this.shipsList.size()<=1)
//			return false;
//		Island isle=(Island) this.target.getObject();
//		if(this.isCapturing()&&!this.isTheIslandCapturingInDanger())
//			return true;
//		if((!this.isCapturing())&&!MyBot.myEnemy.isTargetedIslandByEnemy(this.target))
//			return true;
//		if(this.numOfEnemiesInRange(7)<this.getSize()&&MyBot.game.distance(MyBot.game.getMyPirate(this.id).getLocation(),this.target.getLocation())<isle.getCaptureTurns())
//			return true;
//		if(!MyBot.myEnemy.isTargetedIslandByEnemy(this.target))
//			return true;
//	return false;
	}
	public int numOfEnemiesInRange(int r)
	{
		int count=0;
		for(Pirate erick:MyBot.game.allEnemyPirates())
		{
			if(MyBot.game.distance(MyBot.game.getMyPirate(this.id),erick)<=r)
				count++;
		}
		return count;
	}
	
}

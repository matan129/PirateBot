package bots;

import java.util.*;

import pirates.game.*;

public class EnemyNavy {
	public List<EnemyPirate> shipList;
	private PirateGame game=MyBot.game;
	private int id;
	private int target;
	private Location loc;
    
    public EnemyNavy() {
        this.shipList = new ArrayList<EnemyPirate>();
        this.target=-1;
    }
	public void setLoc(Location loc) {
    	this.loc=loc;
    }
    public Location getLoc() {
    	return this.loc;
    }
    public void setTarget(Island isle) {
    	this.target=isle.getId();
    }
    public void setTarget(int isle) {
    	this.target=isle;
    }
    public int getTarget() {
    	return this.target;
    }
    public boolean isFit(EnemyPirate enemyPirate) {
    	//checks if a pirate fits to the navy
    	//TODO : Simplify this function
    	if (this.shipList.size()==0||this.target==-1)
            return true;
        return (enemyPirate.getTarget() == this.shipList.get(0).getTarget() && this.isPirateInRangeOfNavy(MyBot.game.getEnemyPirate(enemyPirate.getID())));
    }
    public boolean isPirateInRangeOfNavy(Pirate erick) {
    	for(EnemyPirate enemy:this.shipList) {
    		if(MyBot.game.inRange(MyBot.game.getEnemyPirate(enemy.getID()), erick.getLocation()))
    			return true;
    	}
    	return false;
    }
    public boolean isLocationInRangeOfNavy(Location loc) {
    	for(EnemyPirate enemy : this.shipList) {
    		if(MyBot.game.inRange(MyBot.game.getEnemyPirate(enemy.getID()), loc)) {
    			return true;
    		}
    	}
    	return false;
    }
    public void addShip(EnemyPirate enemyPirate) {
        this.shipList.add(enemyPirate);
    }
    public void clearNavy() {
        this.shipList.clear();
    }
    public int getSize() {
        return this.shipList.size();
    }
    public void setID(int id) {
    	this.id=id;
    }
    public int getID() {
    	return this.id;
    }
    public Location getFirstsLocation() {
    	if(this.shipList.size()>0) {
    		return this.shipList.get(0).getLocation();
    	}
    	return null;
    }
    public EnemyPirate getLeadingPirate() {
    	//Initializing
    	Hashtable<Direction, Integer> directionCount=new Hashtable<Direction, Integer>();
    	directionCount.put(Direction.NORTH, 0);
    	directionCount.put(Direction.WEST, 0);
    	directionCount.put(Direction.EAST, 0);
    	directionCount.put(Direction.SOUTH, 0);
    	Direction dir;
    	//Adding possible directions to the hashtable
    	for(EnemyPirate enemy : this.shipList) {
    		dir=game.getDirections(enemy.getPirate(), enemy.getEstimatedTarget().getLocation()).get(0);
    		for(Direction myDir : directionCount.keySet()) {
    			if(dir==myDir) {
    				directionCount.put(myDir, directionCount.get(myDir)+1);
    			}
    		}
    	}

    	//getting the direction with the largest value
    	Direction navyDir=null;
    	Integer maxValue=Integer.MIN_VALUE;
    	for(Map.Entry<Direction,Integer> entry : directionCount.entrySet()) {
    	     if(entry.getValue() > maxValue) {
    	         maxValue = entry.getValue();
    	         navyDir = entry.getKey();
    	     }
    	}
    	//getting the leading pirate using that direction
    	Pirate leadingPirate=this.shipList.get(0).getPirate();
    	for(EnemyPirate enemy : this.shipList) {
    		Pirate myEnemy=enemy.getPirate();
    		switch (navyDir) {
			case NORTH:
				if(myEnemy.getLocation().row<leadingPirate.getLocation().row) {
					leadingPirate=myEnemy;
				}
				break;
			case SOUTH:
				if(myEnemy.getLocation().row>leadingPirate.getLocation().row) {
					leadingPirate=myEnemy;
				}
				break;
			case EAST:
				if(myEnemy.getLocation().col>leadingPirate.getLocation().col) {
					leadingPirate=myEnemy;
				}
				break;
			case WEST:
				if(myEnemy.getLocation().col<leadingPirate.getLocation().col) {
					leadingPirate=myEnemy;
				}
				break;
			default:
				break;
			}
    	}
    	EnemyPirate myLeadingPirate=new EnemyPirate(leadingPirate);
    	return myLeadingPirate;
    }
    public Location getLocation() {
    	return this.getLeadingPirate().getLocation();
    }
    public Island getEstimatedTarget() {
    	Hashtable<Island, Integer> piratesTarget=new Hashtable<Island, Integer>();
    	//Getting targets from all navy's pirates
    	for(EnemyPirate myEnemy : this.shipList) {


    		Island thisTarget=myEnemy.getEstimatedTarget();
    		if(piratesTarget.containsKey(thisTarget)) {
    			piratesTarget.put(thisTarget, piratesTarget.get(thisTarget)+1);
    		}
    		else {
    			piratesTarget.put(thisTarget, 1);
    		}
    	}
    	//Deciding which target is the navy's target
    	Island navyTarget=null;
    	Integer maxValue=Integer.MIN_VALUE;
    	for(Map.Entry<Island,Integer> entry : piratesTarget.entrySet()) {
    		if(entry.getValue() > maxValue) {
    			maxValue = entry.getValue();
    			navyTarget = entry.getKey();
    		}
    	}
    	
    	return navyTarget;
    }
    public boolean isCapturing(Island isle) {
        for(EnemyPirate enemy : this.shipList) {
        	if(enemy.getLocation()==isle.getLocation()) {
        		return true;
        	}
        }
        return false;
    }
    public int turnsToReachTarget() {
    	int mindis=1000000000;
    	for(EnemyPirate erick:this.shipList) {
    		mindis=Math.min(mindis, erick.turnsToReachTarget());
    	}
    	return mindis;
    }
    public Location getLocation1()
    {
    	if(this.shipList.size()==0)
    		return null;
    	int min=1000000000;
    	Location loc=null;
    	for(EnemyPirate erick: this.shipList)
    	{
    		if(MyBot.game.distance(erick.getLocation(), MyBot.game.getIsland(this.target).getLocation())<min)
    		{
    			min=MyBot.game.distance(erick.getLocation(), MyBot.game.getIsland(this.target).getLocation());
    			loc=erick.getLocation();
    		}
    	}
    	return loc;
    }
    public boolean isCapturing() {    	
			Island isle=game.getIsland(this.target);
			if(isle.getTeamCapturing()==Constants.ME && this.isOnTarget()) {
				return true;
			}
       return false;       
    }
    public boolean isOnTarget() {
        for (EnemyPirate myEnemyPirate : shipList) {
        	if (MyBot.game.distance(myEnemyPirate.getLocation(),game.getIsland(target).getLocation())<=1) {
                return true;
            }
        }
        return false;
    }
}

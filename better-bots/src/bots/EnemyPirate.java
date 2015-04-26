package bots;

import java.util.*;

import pirates.game.*;

public class EnemyPirate {

	private int id; 
	List<Location> locationHistory=null;
	private EnemyNavy myNavy;
	private int target=-1;
	
	public EnemyPirate(Pirate pirate) {
		this.locationHistory = new ArrayList<Location>();
        this.id = pirate.getId();
	}
	public int getTarget() {
		return this.target;
	}
	public void setTarget(int tr) {
		this.target=tr;
	}
    public void addLocation() {
        locationHistory.add(0,MyBot.game.getEnemyPirate(this.id).getLocation()); //it adds it to the start of the list
//      MyBot.game.debug("Location History : "+locationHistory.toString());
    }
    public int getID() {
        return this.id;
    }
    public void setNavy(EnemyNavy yourNavy) {
    	this.myNavy=yourNavy;
    }
    public EnemyNavy getNavy() {
    	return this.myNavy;
    }
    public List<Location> getLocationList() {
        return this.locationHistory;
    }
    public Pirate getPirate() {
    	return MyBot.game.getEnemyPirate(this.id);
    }
    public Location getLocation() {
        return MyBot.game.getEnemyPirate(this.id).getLocation();
    }
    public boolean isDead() {
       if(MyBot.game.getEnemyPirate(this.id)==null) {
    	   return true;
       }
       return MyBot.game.getEnemyPirate(this.id).isLost();
    }
    public boolean isCloacked() {
        return MyBot.game.getEnemyPirate(this.id).isLost();
    }
    public int turnsToRevive() {
        return MyBot.game.getEnemyPirate(this.id).getTurnsToRevive();
    }
    public void clearLocations() {
        this.locationHistory.clear();
    }
    public int turnsToReachTarget() {
        return MyBot.game.distance(MyBot.game.getEnemyPirate(this.id), this.getEstimatedTarget());
    }
    public int numberOfEnemiesNearPirateIncludingThePirate() {
        Pirate pirate = MyBot.game.getEnemyPirate(this.id);
        int count=0;
        for (Pirate enemy : MyBot.game.enemyPirates()) {
            if(MyBot.game.inRange(pirate,enemy)) {
                count++;
            }
        }
        return count;
    }
    public Island getEstimatedTarget() {
    	int numberOfTurnsToCheck=3;
        if (this.locationHistory.size() <= numberOfTurnsToCheck) { 
            return this.closestIsland( MyBot.game.islands());
        }
        else {
            List<Island> possibleTargets = new ArrayList<Island>();
            for  (Island isle : MyBot.game.islands()) {
                int[] distances = new int[numberOfTurnsToCheck];// amounts of turns to check, we need to decide
                for (int i = 0; i < distances.length; i++) {
                    distances[i] = MyBot.game.distance(isle.getLocation(), this.locationHistory.get(i));
                }
                int count = 0;
                for (int i = 1; i < distances.length; i++) {
                    if (distances[i] > distances[i - 1]) {
                        count++;
                    }
                }
                if (count > numberOfTurnsToCheck / 2) {
                    possibleTargets.add(isle);
                }
            }
            if(possibleTargets.size()==0)
            	possibleTargets=MyBot.game.islands();
            Island america=this.closestIsland(possibleTargets);
            this.target=america.getId();
            return america;
        }
    }
	public Island closestIsland(List<Island> list) {
		int mindis = 100000;
		if(list.isEmpty()||list==null) {
			return null;
		}
		Island closestIsland1 = list.get(0);
		for (Island isle : list) {
		    if (MyBot.game.distance(MyBot.game.getEnemyPirate(this.id), isle) < mindis) {
		        mindis = MyBot.game.distance(MyBot.game.getEnemyPirate(this.id), isle);
		        closestIsland1 = isle;
		    }
		}
		return closestIsland1;
	}
	
    
}

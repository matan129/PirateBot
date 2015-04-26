package bots;

import java.util.*;
import pirates.game.*;

public class Enemy {
	public static List<EnemyNavy> enemyNavyList=new ArrayList<EnemyNavy>();
    private List<EnemyPirate> shipList=new ArrayList<EnemyPirate>();
    

    public Enemy() {
    	for (Pirate Eldar : MyBot.game.enemyPirates()) {
            EnemyPirate Erick = new EnemyPirate(Eldar);
            this.shipList.add(Erick);
            EnemyNavy myEnemyNavy = new EnemyNavy();
            enemyNavyList.add(myEnemyNavy);
            myEnemyNavy.setID(enemyNavyList.indexOf(myEnemyNavy));
        }
    }
    public void updateEnemy() {
    	
    	for (EnemyNavy navy : enemyNavyList) {
            navy.clearNavy();
        	//FIXME : change target so it won't be -1 at the first turn
            navy.setTarget(-1);
        }
        for (EnemyPirate Erick : this.shipList) {
        	if(!Erick.isDead()) {
        		Erick.addLocation();
            	Erick.getEstimatedTarget();
            	for (EnemyNavy navy : enemyNavyList) {
        			if (navy.isFit(Erick)) {
        				navy.addShip(Erick);
        				Erick.setNavy(navy);
        				navy.setTarget(navy.shipList.get(0).getTarget());
        				break;
        			}
        			
        		}
        	}
			else {
				// if Erick is dead			
				Erick.clearLocations();
				Erick.setTarget(-1);
			}
        }
       this.debugTesting();
       for(EnemyNavy enavy : enemyNavyList)
       {
    	   if(enavy.getSize()>0&&enavy.getTarget()!=-1){
    		   enavy.setLoc(enavy.getLocation1());
    	   }
       }
    }
    public List<EnemyNavy> getNaviesOnWayToTarget(Island target) {
    	List<EnemyNavy> naviesList=new ArrayList<EnemyNavy>();
    	for(EnemyNavy myEnemyNavy : enemyNavyList) {

    		if(myEnemyNavy.getEstimatedTarget()==target) {
    			naviesList.add(myEnemyNavy);
    		}
    	}
    	return naviesList;
    }
    public EnemyNavy getEnemyNavyByIslandCapturing(Island isle) {
    	for(EnemyNavy myEnemyNavy : enemyNavyList) {
    		if(myEnemyNavy.isCapturing(isle)) {
    			return myEnemyNavy;
    		}
    	}
    	return null;
    }
    public void ifAllEnemiesAreDead()
    {
    	for(EnemyNavy enavy : enemyNavyList)
    		enavy.clearNavy();
    }
    public List<EnemyPirate> getPirateList()
    {
    	return this.shipList;
    }
    public void debugTesting() {
    	int index=0;
    	for(EnemyNavy thisNavy : enemyNavyList) {
    		MyBot.game.debug("Navy #%s contains %s pirates, and its target is %s",index, thisNavy.shipList.size(),thisNavy.getTarget());
    		//MyBot.game.debug("Navy's location is %s",thisNavy.shipList.get(0).getLocation());
    		index++;
    	}
    }
    public EnemyNavy getNavyByLeadingPirate(EnemyPirate leadingPirate) {
    	for(EnemyNavy myEnemyNavy : enemyNavyList) {
    		if(myEnemyNavy.shipList.contains(leadingPirate)) {
    			return myEnemyNavy;
    		}
    	}
    	return null;
    }
    public int[]getConfiguration() {
    	int i=0;
    	int[]array= new int[enemyNavyList.size()];
    	for(EnemyNavy navy: enemyNavyList) {
    	array[i]=navy.getSize();
    	i++;
    	}
    	return array;
    }
    public boolean isTargetedIslandByEnemy(Target isle) {
    	for(EnemyPirate Erick:this.shipList) {
    		if(Erick.getTarget()==-1) {
    			continue;
    		}
    		if(MyBot.game.getIsland(Erick.getTarget()).getLocation()==isle.getLocation()) {
    			return true;
    		}
    	}
    	return false;
    }
    public boolean isAllDeadEnemy() {
    	int count=0;
    	for(Pirate Erick: MyBot.game.allEnemyPirates()) {
    		if(Erick.isLost())
    			count++;
    	}
    	return count==MyBot.game.allEnemyPirates().size();
    }
}

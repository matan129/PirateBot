package bots;

import java.util.*;

import pirates.game.*;

public class TargetBank {
	private PirateGame game=MyBot.game;
	public static List<Target> targetBank=new ArrayList<Target>();
	
	public TargetBank() {
		
	}
		
	public void updateTargetBank() {
		//clearing current target bank
		targetBank.clear();
		
		//adding islands to the target bank
		for(Island myIsland : game.islands()) {
			if((myIsland.getOwner()!=Constants.ME) || (myIsland.getOwner()==Constants.ME && myIsland.getTeamCapturing()==Constants.ENEMY)) {
				Target isle=new Target(myIsland);
				updateIslandPriority(isle);
				targetBank.add(isle);	
			}
		}
		
		//adding enemy's pirates to the target bank
		//FIXME : Setting EnemyNavy as target 
	/*	for(EnemyNavy myEnemyNavy : Enemy.enemyNavyList) {
			if(myEnemyNavy!= null && myEnemyNavy.shipList!=null && myEnemyNavy.shipList.size()>0) {
				Target enemyNavy=new Target(myEnemyNavy);
				updateEnemyNavyPriority(enemyNavy);
				targetBank.add(enemyNavy);
			}
		}*/
	}
	
	public void updateIslandPriority(Target tr) {
		Island myIsland=game.getIsland(tr.getID());
		if(estimateEnemyPowerNearisland(myIsland)==game.allEnemyPirates().size()) {
			tr.setPriority(Priority.LOWEST);
		}
		if(estimateEnemyPowerNearisland(myIsland)==0) {
			tr.setPriority(Priority.NORNAL);
		}
		if(myIsland.getValue()==Collections.max(MyBot.islandsScored)) {
			tr.changePriority(+1);
		}
		if(myIsland.getValue()==Collections.min(MyBot.islandsScored) && MyBot.islandsScored.size()>2) {
			tr.changePriority(-1);
		}
		if(myIsland.getOwner()==Constants.NO_OWNER) {
			if(myIsland.getTeamCapturing()!=Constants.ENEMY && myIsland.getTeamCapturing()!=Constants.ME) {
				tr.setPriority(Priority.HIGHEST);;
			}
			else {
				tr.changePriority(+1);
			}
		}
		if(myIsland.getTeamCapturing()==Constants.ENEMY) {
			EnemyNavy myEnemyNavy=MyBot.myEnemy.getEnemyNavyByIslandCapturing(myIsland);
			if(myEnemyNavy!=null) {
				tr.setRequirements(myEnemyNavy.getSize()+1);
			}
		}
		if(myIsland.getOwner()==Constants.ME && myIsland.getTeamCapturing()!=Constants.ENEMY)  {
			//changed to getTeamCapturing instead of getCaptureTurns
			tr.setPriority(Priority.LOWEST);
		}
		
//		if(Manager.isTargetedIsland(myIsland)) {
//			tr.setPriority(Priority.LOW);;
//		}
		
	}
	
	public void updateEnemyNavyPriority(Target tr) {
		EnemyNavy myEnemyNavy=(EnemyNavy)tr.getObject();
		tr.setRequirements(myEnemyNavy.getSize()+1);
	}
	
	public int estimateEnemyPowerNearisland(Island thisIsland) {
        int count = 0;
        for (Pirate enemy : game.enemyPirates()) {
            if (game.distance(enemy, thisIsland) <= 8) {
                count++;
            }
        }
        return count;
    }
	
	public static List<Target> cloneTargetBank() {
	    List<Target> clone = new ArrayList<Target>(targetBank.size());
	    for(Target item : targetBank) {
	    	clone.add(item);
	    }
	    return clone;
	}
	public static boolean isTargetTaken (Target tr) {
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
	public static boolean isCurrentTarget(Target tr, Navy myNavy) {
		if(myNavy.getMainTarget()==null) {
			return false;
		}
		if(myNavy.getMainTarget().getType()==tr.getType() && myNavy.getMainTarget().getID()==tr.getID()) {
			return true;
		}
		return false;
	}

}

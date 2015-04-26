package bots;

import java.util.*;

import pirates.game.*;

public class Navigator {
	
	private PirateGame game;
	private Navy myNavy;
	private List<Target> targetBank=TargetBank.cloneTargetBank();
	
	
	public Navigator(Navy myNavy) {
		this.game = MyBot.game;
		this.myNavy=myNavy;
		updateTargetBank(TargetBank.cloneTargetBank());
	}
	public void setMainTarget() {
		List<Target> mainTargetBank=TargetBank.cloneTargetBank();
		Target myTarget=chooseTarget(mainTargetBank, TargetUtilities.MAIN_TARGET);
		game.debug("Target found for navy #%s : ", myNavy.getID());
		game.debug(myTarget.toString());
		myNavy.setMainTarget(myTarget);
		myNavy.setSecondaryTarget(null);
	}
	public void checkPossibilityToSwitchTarget() {
		if(myNavy.getMainTarget()!=null) {
			if(myNavy.getMainTarget().getType().equals(TargetUtilities.ISLAND)) {
				List<Target> checkTargetBank=new ArrayList<Target>();
				for(Target tr : this.targetBank) {
					if(tr.getPriority().getValue()>=myNavy.getMainTarget().getPriority().getValue()) {
						checkTargetBank.add(tr);
					}
				}
				Target targetToCheck=chooseTarget(checkTargetBank, TargetUtilities.MAIN_TARGET);
				if(myNavy.getMainTarget()!=null && myNavy.getMainTarget().getObject()!=null) {
		//			game.debug("Comparing distances");
					int distanceFromMainTarget=game.distance(myNavy.getLocation(), myNavy.getMainTarget().getLocation());
					int distanceFromTargetToCheck=game.distance(myNavy.getLocation(), targetToCheck.getLocation());
					if(targetToCheck.getPriority().getValue()>=myNavy.getMainTarget().getPriority().getValue()) {
		//				game.debug("Checking priority");
						if(distanceFromTargetToCheck<distanceFromMainTarget) {
							if(targetToCheck!=null) {
								this.myNavy.setMainTarget(targetToCheck);
								game.debug("Target was changed - new target is : ");
								game.debug(targetToCheck.toString());
							}
						}
					}
				}
			}
			if(myNavy.getMainTarget().getType().equals(TargetUtilities.ENEMY_NAVY)) {
				game.debug("EnemyNavy was detected. Setting a new target");
				setMainTarget();
			}
		}
	}
	public void setSecondaryTarget() {
		List<Target> secondaryTargetBank=new ArrayList<Target>();
		for(Target tr : this.targetBank) {
			if(tr.getPriority().getValue()>myNavy.getMainTarget().getPriority().getValue()) {
				secondaryTargetBank.add(tr);
			}
		}
		Target targetToCheck=chooseTarget(secondaryTargetBank, TargetUtilities.SECONDARY_TARGET);
		if(myNavy.getMainTarget()!=null) {
			int distanceFromMainTarget=game.distance(myNavy.getLocation(), myNavy.getMainTarget().getLocation());
			int distanceFromTargetToCheck=game.distance(targetToCheck.getLocation(), myNavy.getMainTarget().getLocation());
			if(targetToCheck.getPriority().getValue()>=myNavy.getMainTarget().getPriority().getValue()) {
				if(distanceFromTargetToCheck<distanceFromMainTarget) {
					if(targetToCheck!=null) {
						this.myNavy.setSecondaryTarget(targetToCheck);
						game.debug("Secondary target was added for Navy#%s - ", myNavy.getID());
						game.debug(targetToCheck.toString());
					}
				}
			}
		}
	}
	public Target chooseTarget(List<Target> myTargetBank, String chooseTargetMode) {
		updateTargetBank(myTargetBank);
		Target myTarget=new Target();
		myTarget.setPriority(Priority.WORST);
//		int minDistance=game.getCols()*game.getRows();
		int distance;
		HashSet<Integer> islandDistances=new HashSet<Integer>();
		HashSet<Integer> enemyDistances=new HashSet<Integer>();
		boolean targetFound=false;
		Hashtable<Target, Integer> navyTargets=new Hashtable<Target, Integer>();
		for(Target tr : myTargetBank) {
			distance=game.distance(tr.getLocation(), myNavy.getLocation());
			if(chooseTargetMode.equals(TargetUtilities.MAIN_TARGET)) {
				distance=game.distance(tr.getLocation(), myNavy.getLocation());
			}
			if(chooseTargetMode.equals(TargetUtilities.SECONDARY_TARGET)) {
				distance=game.distance(tr.getLocation(), myNavy.getMainTarget().getLocation());
			}
			if(tr.getType().equals(TargetUtilities.ISLAND)) {
				islandDistances.add(distance);
			}
			if(tr.getType().equals(TargetUtilities.ENEMY_NAVY)) {
				enemyDistances.add(distance);
			}
			navyTargets.put(tr, distance);
		}
		
		// Works on run.bat, doesn't work on site...
//		distanceList.sort((distance1, distance2) -> distance1.compareTo(distance2));
		List<Integer> islandsDistanceList=new ArrayList<Integer>(islandDistances);
		List<Integer> enemyDistanceList=new ArrayList<Integer>(enemyDistances);
		Collections.sort(islandsDistanceList);
		Collections.sort(enemyDistanceList);
//		game.debug(islandsDistanceList.toString());
//		game.debug("index 0 : " + islandsDistanceList.get(0)); //min
//		game.debug("last index : " + islandsDistanceList.get(islandsDistanceList.size()-1)); //max
		int compromiseToCheck=2;
		List<Integer> distanceListToCheck=islandsDistanceList;
		for(Target tr : myTargetBank) {
			game.debug("Checking target for navy #%s : ", myNavy.getID());
			game.debug(tr.toString());
			if(!TargetBank.isCurrentTarget(tr, myNavy)) {
				if(tr.getPriority().getValue()>=myTarget.getPriority().getValue()) {
	//				// tr has lower priority in comparison to myTarget. in that case, we'll move on to check the next one we have
	//				continue;
	//			}
					// checking target type and choosing the right distance list
					switch (tr.getType()) {
					case TargetUtilities.ISLAND:
						distanceListToCheck=islandsDistanceList;
						break;
					case TargetUtilities.ENEMY_NAVY:
						distanceListToCheck=enemyDistanceList;
						break;
					default:
						distanceListToCheck=islandsDistanceList;
						break;
					}
								
					// checking target priority and choosing the right compromiseToCheck
					if(distanceListToCheck.size()>4) {
						switch (tr.getPriority()) {
						case LOWEST:
							compromiseToCheck=2;
							break;
						case LOW:
							compromiseToCheck=2;
							break;
						case NORNAL:
							compromiseToCheck=distanceListToCheck.size()/4;
							break;
						case HIGH:
							compromiseToCheck=distanceListToCheck.size()/3;
							break;
						case HIGHEST:
							compromiseToCheck=distanceListToCheck.size()/3;
							break;
						default:
							compromiseToCheck=2;
							break;
						} 
					}
					else {
						compromiseToCheck=distanceListToCheck.size();
					}
	//				game.debug(tr.getType());
	//				game.debug(distanceListToCheck.toString());
	//				game.debug(Integer.toString(compromiseToCheck));
	//				game.debug(Integer.toString(navyTargets.get(tr)));
					// checking tr in comparison to myTarget
					if(compromiseToCheck>=distanceListToCheck.size() || navyTargets.get(tr)<=distanceListToCheck.get(compromiseToCheck)) { // checking if this target is in our compromise range
						if(tr.getPriority().getValue()>myTarget.getPriority().getValue()) {
							myTarget=tr;
						}
						if(tr.getPriority().getValue()==myTarget.getPriority().getValue()) {
							if(myTarget.getPriority()==Priority.WORST)
								continue;
							if(navyTargets.get(tr)<navyTargets.get(myTarget)) {
								myTarget=tr;
							}
						}
					}
				}
			}
		}
		if(myTarget==null || myTarget.getObject()==null) {
			myTarget=this.myNavy.getMainTarget();
			game.debug("Same target as before. No other targets...");
		}
		return myTarget;
	}
	
	public void updateTargetBank(List<Target> myTargetBank) {
//		this.targetBank.clear();
//		this.targetBank=TargetBank.cloneTargetBank();
		
		for(Target tr : myTargetBank) {
			if(tr.getType().equals(TargetUtilities.ISLAND)) {
				updateIslandPriority(tr);
			}
			if(tr.getType().equals(TargetUtilities.ENEMY_NAVY)) {
				updateEnemyNavyPriority(tr);
			}
			if(TargetBank.isTargetTaken(tr)) {
				tr.setPriority(Priority.LOWEST);
			}
		}
	}

	
	public void updateIslandPriority(Target tr) {
		Island myIsland=game.getIsland(tr.getID());
		if(myIsland.getTeamCapturing()==Constants.ENEMY) {
			EnemyNavy myEnemyNavy=MyBot.myEnemy.getEnemyNavyByIslandCapturing(myIsland);
			if(myEnemyNavy!=null) {
				if(myEnemyNavy.getSize()>myNavy.getSize()) {
					tr.changePriority(-2);
				}
				else {
					tr.changePriority(+2);
	
				}
			}
		}
		for(EnemyNavy myEnemyNavy : Enemy.enemyNavyList) {
			if(myEnemyNavy.getTarget()!=-1) {
				Island enemyTarget=game.getIsland(myEnemyNavy.getTarget());
				if(enemyTarget.equals(myIsland)) {
					if(myEnemyNavy.getSize()>myNavy.getSize()) {
						tr.changePriority(-2);
					}
					else {
						tr.changePriority(+1);
		
					}
				}
			}
		}
	}
	
	public void updateEnemyNavyPriority(Target tr) {
		EnemyNavy myEnemyNavy=(EnemyNavy)tr.getObject();
		if(myEnemyNavy.getSize()>myNavy.getSize()) {
			tr.setPriority(Priority.LOWEST);
		}
		else {
			tr.changePriority(+2);
			if(myEnemyNavy.isCapturing()) {
				tr.changePriority(+1);
			}
		}
	}
}

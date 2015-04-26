package bots;

import java.util.*;
import pirates.game.*;

public class Manager {
	
    private PirateGame game;
    public static List<Navy> naviesList;
    private List<Target> targetBank;
    
    public Manager() {
        this.game = MyBot.game;
        naviesList=new ArrayList<Navy>(game.myPirates().size());
        this.targetBank=TargetBank.cloneTargetBank();
        game.debug("manager");
    }
    
    public void initialization() {
    	setIslandScores();
    	//Initializing setting navies array
    	for(int i=0; i<naviesList.size(); i++) {
    		naviesList.add(new Navy(i));
    	}
    	
    	int numberOfIslands=game.islands().size();
		int numberOfPirates=game.allMyPirates().size();
		//Right now we are going as groups of 3. It will be changed.
		//TODO : changing the number of teams.
		int numberOfTeams=MyBot.game.allMyPirates().size();
		//adding pirates
		int i=0;
		int pirateID=0;
		int teamIndex=0;
		game.debug("naviesList.size() - "+naviesList.size());
		while (pirateID<numberOfPirates) {
			if(naviesList.size()<=teamIndex) {
				naviesList.add(new Navy(i));
//				game.debug("naviesList.size() - "+naviesList.size());
			}
			naviesList.get(teamIndex).addShip(pirateID);
			i++;
			pirateID++;
			teamIndex++;
//			game.debug("teamIndex - " + teamIndex);
			if(teamIndex>=numberOfTeams) {
//				game.debug("teamIndex - "+teamIndex);
				teamIndex=0;
			}
		}
    }
    public int numOfGoodPriorities() {
		int count=0;
		if(targetBank==null) {
			return 0;
		}
		for(Target tr: this.targetBank) {
			if(tr!=null && tr.getPriority()!=null) {
				if(tr.getPriority().getValue()>=3) {
					count++;
				}
			}
		}
		return count;
	}

    public void updateNavies() {
    	// a function that deals with the subject of dynamic navies. not finished yet.
    	MyBot.game.debug("updateNavies");
    	this.deleteLostPirates();
    	this.createNaviesForReturningPirates();
    	this.mergeSimilarNavies();
    	//this.divideNaviesWhenAllEnemiesAreDead();
    	//TODO: add a function that checks if there is a need for pirates (from the navigator).
    	//functions that checks if it's recommended to divide:
    	//this.divideAccordingToGoodPriorities();
    	this.updateNaviesModes();
    	this.updateCloak();
    }
    	
    	public void deleteLostPirates() {
    		MyBot.game.debug("deleteLostPirates");
		    if(MyBot.game.myLostPirates().size()!=0) {
		    	for(Navy navy1 : naviesList) {
		    		//deletes lost pirates
		    		if(!navy1.isEmpty()) {
		    			navy1.deleteLostPirates();
		    		}
		    	}
		    }
    	}
    	public void updateCloak() {
        	boolean cloaked=false;
        	for(Navy navy : naviesList) {
        		if(!cloaked){
        			if(navy.isGhosted()) {
        				navy.setIsCloaked(true);
        				cloaked=true;
        			}
        			else {
        				navy.setIsCloaked(false);
        			}
        		}
        		else{
        			navy.setIsCloaked(false);
        		}
        	}
        }
    	public void createNaviesForReturningPirates() {
    		MyBot.game.debug("createNaviesForReturningPirates");
	    	for(Pirate eldar: MyBot.game.myPirates()) {
	    		// creates a navy for returning pirates or adding them to an existing navy
	    		if(!this.isPartOfANavy(eldar)&&!eldar.isLost()) {
	    			MyBot.game.debug("creating a navy for the reborn");
	    			int count=0;
	    			for(Navy navy1 : naviesList) {
	    				if(navy1.isEmpty()) {
	    					count++;
	    					continue;
	    				}
	    				if(MyBot.game.InRange(navy1.getLocation(), eldar.getLocation())) {
	    					navy1.addShip(eldar.getId());
	    					break;
	    				}
	    				count++;
	    			}
	    			if(count==naviesList.size()) {
	    				for(Navy navy1 : naviesList) {
	    					if(navy1.isEmpty()) {
	    						navy1.addShip(eldar.getId());
	    						navy1.setMode(NavyMode.INITIAL);
	    						break;
	    					}
	    				}
	    			}
	    		}
	    	}
    	}
    	public void mergeSimilarNavies()
        {
        	for(int i=0;i<naviesList.size()-1;i++)//merges similar navies
        	{Navy navy1=naviesList.get(i);
        		if(navy1.isEmpty()||navy1.getMainTarget()==null||!(navy1.getMode()==NavyMode.CONQUERING)||navy1.getIsCloaked())
        		{
        			continue;
        		}
        		for(int j=i+1;j<naviesList.size();j++)
        		{Navy navy2=naviesList.get(j);
        			if(navy2.isEmpty()||navy2.getMainTarget()==null||!(navy2.getMode()==NavyMode.CONQUERING)||navy2.getIsCloaked())
            		{
            			continue;
            		}
        			MyBot.game.debug("checking merge");
        			boolean b1=MyBot.game.InRange(navy1.getLocation(), navy2.getLocation());
        			boolean b2=navy1.getMainTarget()==navy2.getMainTarget();
        			MyBot.game.debug("%s",b1);
        			MyBot.game.debug("%s",b2);
        			if(MyBot.game.InRange(navy1.getLocation(), navy2.getLocation())&&navy1.getMainTarget().getType()==navy2.getMainTarget().getType()&&navy1.getMainTarget().getID()==navy2.getMainTarget().getID())
        			{
        				MyBot.game.debug("merging bool");
        				this.mergeNavies((navy1), navy2);
        			}
        		}
        	} 
        	for(Navy navy:naviesList)
        	{
        		if(navy.getSize()<=1||navy.getMode()==NavyMode.CONQUERING||navy.getMode()==NavyMode.CONQUERING)
        			continue;
        		for(Navy navy2: naviesList)
        		{
        			if(navy2.getSize()!=0)
        				continue;
        			this.movePirateFromOneNavyToAnother(navy, navy2);
        		}
        	} 
        }
		public void divideAccordingToGoodPriorities() {
			MyBot.game.debug("divideAccordingToGoodPriorities");
    	    if(this.numOfNavies()<this.numOfGoodPriorities()) {
    		//if there are more good targets then navies, this function will divide the closest navies to the targets
    		for(Navy navy1 : naviesList) {
    			if(navy1.isDividable()>0) {
    				for(Navy navy2 : naviesList) {
    					if(navy2.isEmpty()) {
    						this.movePirateFromOneNavyToAnother(navy1, navy2);
    					}
    				}
    			}
    		}
    	} 
    }
		public void updateNaviesModes()
	    {MyBot.game.debug("updateNaviesModes");
	    	for(Navy navy:naviesList)
	    	{
	    		if(navy.getMode()==NavyMode.WAITING)
	    		{
	    			continue;
	    		}
	    		if(navy.getShiplist().size()==0||navy.isAllDead()||navy.getMainTarget()==null)
	    		{
	    			navy.setMode(NavyMode.INITIAL);
	    			continue;
	    		}
	    		if(navy.getMode()==NavyMode.CONQUERING)
	    		{
	    			Island isle= MyBot.game.getIsland(navy.getMainTarget().getID());
	    			if(isle.getOwner()!=Constants.ME && navy.isCapturing())
	    				continue;
	    		}
	    		if(navy.getMainTarget().getType().equals(TargetUtilities.ISLAND))
	    		{
	    			if(MyBot.game.getIsland(navy.getMainTarget().getID()).getOwner()==Constants.ME)
	    			{
	    				navy.setMode(NavyMode.INITIAL);
	    				continue;
	    			}
	    			if(navy.isCapturing())
	    			{
	    				navy.setMode(NavyMode.CONQUERING);
	    				continue;
	    			}
	    			navy.setMode(NavyMode.SAILING);
	    			continue;
	    		}
	    		if(navy.getMainTarget().getType().equals(TargetUtilities.ENEMY_NAVY)) {
	    			Pirate pir=game.getEnemyPirate(navy.getMainTarget().getID());
	    			if(pir.isLost() || pir.isCloaked()) 
	    			{
	    				navy.setMode(NavyMode.INITIAL);
	    				continue;
	    			}
	    			else
	    			{
	    				navy.setMode(NavyMode.SAILING);
	    				continue;
	    			}
	    		}
	    	}
	    }
    public void divideNaviesWhenAllEnemiesAreDead() {
		if(!MyBot.myEnemy.isAllDeadEnemy()) {
    		return;
		}
    	for(Pirate eldar:MyBot.game.myPirates()) {
			for(Navy navy : naviesList) {
				navy.removeShip(eldar);
			}
		}
		int i=0;
		for(Pirate eldar:MyBot.game.myPirates()) {
			naviesList.add(new Navy(i));
			naviesList.get(i).addShip(eldar);
			i++;
		}
    }
    public boolean isPartOfANavy(Pirate eldar) {
    	int eldarid=eldar.getId();
    	for(Navy nav : naviesList) {
    		if(nav.getShiplist().contains(eldarid))
    			return true;
    	}
    	return false;
    }
    public void movePirateFromOneNavyToAnother(int id1, int id2, Location loc) {
    	Navy navy1=this.getNavyByID(id1);
    	Navy navy2= this.getNavyByID(id2);
    	Pirate eldar=navy1.closestPirateToTarget(loc);
    	navy1.removeShip(eldar);
    	navy2.addShip(eldar.getId());
    }
    public void movePirateFromOneNavyToAnother(Navy navy1, Navy navy2) {
    	if(navy1.getShiplist().size()==0) {
    		return;
    	}
    	Pirate eldar=navy1.closestPirateToTarget(null);
    	navy1.removeShip(eldar);
    	navy2.addShip(eldar.getId());
    }
    public Navy getNavyByID(int id) {
    	return  naviesList.get(id);
    }
    public void mergeNavies(Navy navy1,Navy navy2) {
    	MyBot.game.debug("merging func");
    	for(Pirate eldar:MyBot.game.myPirates()) {
    		if(navy2.getShiplist().contains(eldar.getId())) {
    			navy2.removeShip(eldar);
    			navy1.addShip(eldar);
    		}
    	 }
     }
    
    public void setIslandScores () {
    	for(Island isle : game.islands()) {
    		MyBot.islandsScored.add(isle.getValue());
    		game.debug("Island #%s, score %s", isle.getId(), isle.getValue());
    	}
    }
    
    public void regrouping(int minPriority) {
    	
    	// getting information about available pirates from all navies
    	int availablePirates=0;
    	Hashtable<Navy, Integer> portablePirates=new Hashtable<Navy, Integer>();
    	for(Navy myNavy : naviesList) {
			if(myNavy.isDividable()>0) {
				portablePirates.put(myNavy, myNavy.isDividable());
				availablePirates++;
			}
		}
    	  	
    	// setting minimum priority to take care of to priority 4
    	List<Target> targetBankToCheck=sortTargetBankByRequirementsAndPriority(minPriority);
    	
    	for(Target tr : targetBankToCheck) {
    		if(tr.getRequirements()<=availablePirates) {
    			List<Navy> navyListByProximity=sortNavyListByProximityToTarget(tr, portablePirates.keySet());
    			for(int i=0; i<navyListByProximity.size(); i++) {
    				Navy myNavy=navyListByProximity.get(i);
    				if(portablePirates.get(myNavy)!=null) {
    					game.debug(portablePirates.get(myNavy).toString());
	    				if(portablePirates.get(myNavy)>0) {
	    					List<Pirate> thisNavyPoratablePirates=getPortablePiratesList(myNavy, portablePirates.get(myNavy));
	    					if(getNavyToAssist(tr) != null) { 
	    						split(thisNavyPoratablePirates, getNavyToAssist(tr), myNavy);
	    						availablePirates-=thisNavyPoratablePirates.size();
	    						portablePirates.remove(myNavy);
	    					}
	    					else {
	    						split(thisNavyPoratablePirates, tr, myNavy);
	    						availablePirates-=thisNavyPoratablePirates.size();
	    						portablePirates.remove(myNavy);
	    					}
	    				}
	    			}
    			}
    		}
    	}
    }
    public List<Pirate> getPortablePiratesList (Navy contributorNavy, int numberOfPortablePirates) {
    	List<Pirate> portablePiratesList=new ArrayList<Pirate>();
    	for(int i=0; i<contributorNavy.getShiplist().size() && i<numberOfPortablePirates; i++) {
    		Pirate portablePirate=game.getMyPirate(i);
    		if(!MyBot.isOnIsland(portablePirate.getLocation())) {
    			portablePiratesList.add(portablePirate);
    		}
    	}
    	return portablePiratesList;
    }
    public static List<Navy> cloneNaviesList() {
	    List<Navy> clone = new ArrayList<Navy>(naviesList.size());
	    for(Navy item : naviesList) {
	    	clone.add(item);
	    }
	    return clone;
	}
    public List<Navy> sortNavyListByProximityToTarget(Target tr, Set<Navy> navyListToCheck) {
    	List<Navy> sortedNavyList=new ArrayList<Navy>();
    	int closestDistance=Integer.MAX_VALUE;
    	Navy closestNavy=null;
    	while(navyListToCheck.size()>0) {
	    	for(Navy myNavy : navyListToCheck) {
	    		if(game.distance(myNavy.getLocation(), tr.getLocation())<=closestDistance) {
	    			closestDistance=game.distance(myNavy.getLocation(), tr.getLocation());
	    			closestNavy=myNavy;
	    		}
	    	}
	    	sortedNavyList.add(closestNavy);
	    	closestDistance=Integer.MAX_VALUE;
	    	navyListToCheck.remove(closestNavy);
    	}
    	return sortedNavyList;
    }
    public Navy getNavyToAssist(Target tr) {
    	for(Navy myNavy : naviesList) {
    		if(TargetBank.isCurrentTarget(tr, myNavy)) {
    			return myNavy;
    		}
    	}
    	return null;
    }
    public void split(List<Pirate> portablePirates, Target destinationTarget, Navy originalNavy) {
    	Navy travelingNavy = new Navy();
    	for(Pirate portablePirate : portablePirates) {
    		travelingNavy.addShip(portablePirate);   		
    	}
    	travelingNavy.setMainTarget(destinationTarget);
    	originalNavy.getShiplist().removeAll(portablePirates);
    }
    
    public void split(List<Pirate> portablePirates, Navy destinationNavy, Navy originalNavy) {
    	Navy travelingNavy = new Navy();
    	for(Pirate portablePirate : portablePirates) {
    		travelingNavy.addShip(portablePirate);   		
    	}
    	travelingNavy.setSecondaryTarget(destinationNavy.getMainTarget());
    	Target destinationTarget=new Target(destinationNavy);
    	travelingNavy.setMainTarget(destinationTarget);
    	originalNavy.getShiplist().removeAll(portablePirates);
    }
    
    public List<Target> sortTargetBankByRequirementsAndPriority(int minPriority) {
    	List<Target> targetBankToCheck=getTargetBankForRegrouping(minPriority);
    	//setting an array for sorted targetBank for each possible priority
    	List<Target>[] targetBankArrayByPriority=new ArrayList[Priority.getNumberOfPossiblePriority()-minPriority];
    	for(int i=0; i<targetBankArrayByPriority.length; i++) {
    		targetBankArrayByPriority[i]=new ArrayList<Target>();
    	}
    	//setting a new empty list for sorted targetBank
    	List<Target> sortedTargetBank=new ArrayList<Target>();
    	int minValue=Integer.MAX_VALUE;
    	Target minRequirementsTarget=null;
    	//getting the lowest requirement
    	while(this.targetBank.size()>0) {
	    	for(Target tr : targetBankToCheck) {
	    		if(!TargetBank.isTargetTaken(tr)) {
		    		if(tr.getRequirements()<minValue) { 
		    			minRequirementsTarget=tr;
		    		}
	    		}
	    	}
	    	if(minRequirementsTarget!=null) {
	    		//FIXME : Check why there are sometimes where it's null
		    	targetBankArrayByPriority[minRequirementsTarget.getPriority().getValue()].add(minRequirementsTarget);
		    	this.targetBank.remove(minRequirementsTarget);
	    	}
	    	minValue=Integer.MAX_VALUE;
    	}
    	//going over the array and adding targets into sorted list
    	for(int i=targetBankArrayByPriority.length-1; i>=0; i--) {
    		for(Target tr : targetBankArrayByPriority[i]) {
    			sortedTargetBank.add(tr);   			
    		}
    	}
    	return sortedTargetBank;
    }
    
    public List<Target> getTargetBankForRegrouping(int minPrioirty) {
    	List<Target> myTargetBank=new ArrayList<Target>();
    	for(Target tr : this.targetBank) {
    		if(tr.getPriority().getValue()>=minPrioirty) {
    			myTargetBank.add(tr);
    		}
    	}
    	return myTargetBank;
    }
    
    public Navy findNearestNavy(Target tr, List<Navy> possibleNavyList) {
    	Navy nearestNavy=null;
    	int minDistance=Integer.MAX_VALUE;
    	for(Navy myNavy : possibleNavyList) {
    		if(game.distance(myNavy.getLocation(), tr.getLocation())<minDistance) {
    			minDistance=game.distance(myNavy.getLocation(), tr.getLocation());
    			nearestNavy=myNavy;
    		}
    	}
    	return nearestNavy;
    }
    
    public void merge(Navy targetNavy, Navy sourceNavy) {
    	List<Pirate> portablePiratesDeleteList=new ArrayList<Pirate>();
    	for(Integer pirateID : sourceNavy.getShiplist()) {
    		Pirate portablePirate=game.getMyPirate(pirateID);
    		targetNavy.addShip(portablePirate);
    		portablePiratesDeleteList.add(portablePirate);
    	}
    	sourceNavy.getShiplist().removeAll(portablePiratesDeleteList);
    }
//    
//    
//    public void initDistribute(int[] configuration)// divides the pirates into navies
//    {
//        int k = 0;
//        for (int i = 0; i < configuration.length; i++)
//        {
//            Navy navy = new Navy();
//            navy.setGame(game);
//            for ( int j=k ; j < configuration[i]+k; j++)
//            {
//                navy.addShip(j);
//            }
//            k = k + configuration[i];
//            this.navies.add(navy);
//        }
//        MyBot.game.debug("finish distribution");
//    }
    public void setGame(PirateGame game) {
        this.game = game;
    }
    public void play() {
        MyBot.game.debug("Turn : %s", MyBot.game.getTurn());
        regrouping(4);
        if(naviesList.size()>0) {
	        for (int i=0; i<naviesList.size(); i++) {
	        	Navy navy=naviesList.get(i);
	        	MyBot.game.debug("%s", navy.getMode());
	        	MyBot.game.debug("%s",navy.getSize());
	        	if(navy!=null && navy.getSize()>0 && !navy.isAllDead()) {
	        		navy.myNav.checkPossibilityToSwitchTarget();
	        		navy.myNav.setSecondaryTarget();
		            if (navy.getMode()==NavyMode.INITIAL)  {
//		                game.debug("not busy");
		            	navy.myNav.setMainTarget();
		                if(navy.getMainTarget()==null)
		                	continue;
		                if(navy.getMainTarget().getType().equals(TargetUtilities.ISLAND))
		                	navy.setMode(NavyMode.SAILING);
		            }
		            if(navy.getSecondaryTarget()==null) {
		            	MyBot.sailor.setSail(navy, TargetUtilities.MAIN_TARGET);
		            }
		            else {
		            	MyBot.sailor.setSail(navy, TargetUtilities.SECONDARY_TARGET);
		            }
		            resetSecondaryTargetIfDone(navy);
	        	}
	        }
        } 
    }
    public void resetSecondaryTargetIfDone(Navy myNavy) {
    	if(myNavy.getSecondaryTarget()!=null && myNavy.getSecondaryTarget().getType()!=null) {
    		if(myNavy.getSecondaryTarget().getType().equals(TargetUtilities.ISLAND)) {
    			Island isle=game.getIsland(myNavy.getSecondaryTarget().getID());
    			if(isle.getTeamCapturing()==Constants.ME) {
    				myNavy.setSecondaryTarget(null);
    				return;
    			}
    		}
    		if(myNavy.getSecondaryTarget().getType().equals(TargetUtilities.ENEMY_PIRATE)){
    			EnemyPirate myEenmyPirate=(EnemyPirate)myNavy.getSecondaryTarget().getObject();
    			Pirate Erick=game.getEnemyPirate(myEenmyPirate.getID());
    			if(Erick.isCloaked() || Erick.isLost()) {
    				myNavy.setSecondaryTarget(null);
    				return;
    			}
    		}
    		if(myNavy.getSecondaryTarget().getType().equals(TargetUtilities.ENEMY_NAVY)) {
    			EnemyNavy myEnemyNavy=(EnemyNavy)myNavy.getSecondaryTarget().getObject();
    			if(myEnemyNavy.shipList==null || myEnemyNavy.shipList.size()==0) {
    				myNavy.setSecondaryTarget(null);
    				return;
    			}
    		}
       	}
    }
 /*   public boolean isTargetedIsland(Island isle) {
        for (int i=0; i<this.naviesArr.length; i++) {
        	Navy navy=this.naviesArr[i];
            if (navy.getTarget() == isle) {
                return true;
           }
        }
        return false;
    } */
    public int numOfNavies() {
        /*	int count=0;
        	for(Navy navy1 : naviesList) {
        		if(navy1.getSize()>0)
        			count++;
        	}
        	return count;
        */
        	return naviesList.size();
    }
	
}

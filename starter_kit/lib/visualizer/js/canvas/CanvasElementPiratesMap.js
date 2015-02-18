/**
 * @class The main map including pirates and indicators
 * @extends CanvasElement
 * @constructor
 * @param {State}
 *        state the visualizer state for reference
 * @param {CanvasElementMap}
 *        map the background map
 * @param {CanvasElementFog}
 *        fog the fog overlay
 */
function CanvasElementPiratesMap(state, map, fog) {
	this.upper();
	this.state = state;
	this.map = map;
	this.fog = fog;
	this.dependsOn(map);
	this.dependsOn(fog);
	this.time = 0;
	this.pirates = [];
	this.drawStates = new Object();
	this.pairing = [];
	this.scale = 1;
	this.circledPirates = [];
	this.mouseOverVis = false;
	this.mouseCol = 0;
	this.mouseRow = 0;
	this.islandImage = null;
	this.ownedIslandSprites = null;
}
CanvasElementPiratesMap.extend(CanvasElement);

/**
 * Causes a comparison of the relevant values that make up the visible content of this canvas
 * between the visualizer and cached values. If the cached values are out of date the canvas is
 * marked as invalid.
 *
 * @returns {Boolean} true, if the internal state has changed
 */
CanvasElementPiratesMap.prototype.checkState = function() {
	var i, k, kf, p_i, p_k, dx, dy, rows, cols, ar, owner;
	var hash = undefined;
	var timeChanged = this.time !== this.state.time;
	if (timeChanged || this.scale !== this.state.scale || this.label !== this.state.config['label']) {
		this.invalid = true;
		this.time = this.state.time;
		this.scale = this.state.scale;
		this.label = this.state.config['label'];

		// per turn calculations
		if (this.turn !== (this.time | 0)) {
			cols = this.state.replay.cols;
			rows = this.state.replay.rows;
			this.turn = this.time | 0;
			this.pirates = this.state.replay.getTurn(this.turn);
			this.pairing = new Array(this.pirates.length);
			for (i = this.pirates.length - 1; i >= 0; i--) {
				if ((kf = this.pirates[i].interpolate(this.turn))) {
					owner = kf['owner'];
					kf = this.pirates[i].interpolate(this.turn + 1);
					this.pairing[this.pirates[i].id] = {
						kf : kf,
						owner : owner,
						x : Math.wrapAround(kf['x'], cols),
						y : Math.wrapAround(kf['y'], rows),
						targets : []
					};
				}
			}
			if ((ar = this.state.replay.meta['replaydata']['attackradius2'])) {
				for (i = this.pirates.length - 1; i >= 0; i--) {
					if (this.pirates[i].death === this.turn + 1) {
						p_i = this.pairing[this.pirates[i].id];
						if (p_i !== undefined && p_i.owner !== undefined) {
							for (k = this.pirates.length - 1; k >= 0; k--) {
								// this check looks odd, but accounts for
								// surviving pirates
								if (this.pirates[k].death !== this.turn + 1 || k < i) {
									p_k = this.pairing[this.pirates[k].id];
									if (p_k !== undefined && p_k.owner !== undefined
											&& p_i.owner !== p_k.owner) {
										// distance between pirates' end-points
										dx = Math.wrapAround(p_k.x - p_i.x, cols);
										if (2 * dx > cols) dx -= cols;
										dy = Math.wrapAround(p_k.y - p_i.y, rows);
										if (2 * dy > rows) dy -= rows;
										if (dx * dx + dy * dy <= ar) {
											// these two pirates will be in attack
											// range
											p_i.targets.push(p_k.kf);
											p_k.targets.push(p_i.kf);
										}
									}
								}
							}
						}
					}
				}
			}
		}

		// interpolate pirates for this point in time
		this.drawStates = new Object();
		for (i = this.pirates.length - 1; i >= 0; i--) {
			if ((kf = this.pirates[i].interpolate(this.time))) {
				hash = '#';
				hash += INT_TO_HEX[kf['r']];
				hash += INT_TO_HEX[kf['g']];
				hash += INT_TO_HEX[kf['b']];
				kf.calcMapCoords(this.scale, this.w, this.h);
				if (!this.drawStates[hash]) this.drawStates[hash] = [];
				this.drawStates[hash].push(kf);
			}
		}
	}

	// find pirates in range of mouse cursor
	if (this.mouseOverVis !== this.state.mouseOverVis
			|| this.mouseOverVis
			&& (timeChanged || this.mouseCol !== this.state.mouseCol || this.mouseRow !== this.state.mouseRow)) {
		this.mouseOverVis = this.state.mouseOverVis;
		this.mouseCol = this.state.mouseCol;
		this.mouseRow = this.state.mouseRow;
		if (this.collectPiratesAroundCursor()) this.invalid = true;
	}
};

/**
 * Builds the internal list of pirates and food that need a circle drawn around them because the mouse
 * cursor is within their radius of effect (either attack or spawn).
 *
 * @returns {Boolean} true, if the internal list has changed since the last call of this method
 */
CanvasElementPiratesMap.prototype.collectPiratesAroundCursor = function() {
	var col, row, ar, sr, colPixels, rowPixels, drawList, i, k, pirate, d, owned;
	var found;
	var circledPirates = [];
	var hash = undefined;
	var same = true;
	if (this.mouseOverVis) {
		col = this.scale * this.mouseCol;
		row = this.scale * this.mouseRow;
		ar = this.state.replay.meta['replaydata']['attackradius2'];
		ar *= this.scale * this.scale;
		sr = this.state.replay.meta['replaydata']['spawnradius2'];
		sr *= this.scale * this.scale;
		colPixels = this.scale * this.state.replay.cols;
		rowPixels = this.scale * this.state.replay.rows;
		for (hash in this.drawStates) {
			drawList = this.drawStates[hash];
			for (i = drawList.length - 1; i >= 0; i--) {
				pirate = drawList[i];
				d = Math.dist_2(col, row, pirate.mapX, pirate.mapY, colPixels, rowPixels);
				owned = pirate['owner'] !== undefined;
				if (!owned && (d <= sr) || owned && (d <= ar)) {
					if (same) {
						found = false;
						for (k = 0; k < this.circledPirates.length; k++) {
							if (this.circledPirates[k] === pirate) {
								found = true;
								break;
							}
						}
						same &= found;
					}
					circledPirates.push(pirate);
				}
			}
		}
	}
	same &= circledPirates.length === this.circledPirates.length;
	if (same) return false;
	this.circledPirates = circledPirates;
	return true;
};

/**
 * Draws pirates onto the map image. This includes overlay letters / ids, attack lines, effect circles
 * and finally the fog of war.
 */
CanvasElementPiratesMap.prototype.draw = function() {
	var halfScale, drawList, n, kf, w, dx, dy, d, fontSize, label, caption, order;
	var target, rows, cols, x1, y1, x2, y2, rowPixels, colPixels, ar, sr, r, island, islands, i;
    var attack_history, entry, prev_owner, px1, py1;
	var hash = undefined;
    // i added this whole line
    var owners, cur_turn, j, status, turn, owner;

	// draw map
	this.ctx.drawImage(this.map.canvas, 0, 0);
	//this.ctx.drawImage(this.map.board, 0, 0);

    // islands
    halfScale = 0.5 * this.scale;
	islands = this.state.replay.meta['replaydata']['forts'];
    for (i = 0; i < islands.length; i++) {
        island = islands[i];
        x1 = (island[1] - 2.5) * this.scale;
		y1 = (island[0] - 4) * this.scale;
		x2 = (island[1] + 0.5) * this.scale;
		y2 = (island[0] + 0.5) * this.scale;
        w = 5 * this.scale;
        
        // find the current owner. iterate through states until you get to a future state, then just take the previous most one.
        owners = island[2];
        owner = owners[0][1];
        j = 1;
        for (j = 1; j < owners.length; j++) {
            status = owners[j];
            if (this.turn >= status[0]) {
                owner = status[1];
            } else {
                break;
            }
        }
        
        // Generate a progress-bar over island being captured
        // this.ctx.globalAlpha is always 1. use it to fade in. 
        // this if causes code to be skipped in games that dont have this data structure
        this.ctx.lineWidth = 0.5 * this.scale;
        neutral_color_index = this.state.options.playercolors.length - 1;
        if (island.length > 3) {
            attack_history = island[3];
            entry = null;
            for (j = 0; j < attack_history.length; j++) {
                entry = attack_history[j];
                if ((this.turn >= entry[1]) && (this.turn < entry[1]+entry[2] - 1)) {
                    // Background always NEUTRAL
                    this.ctx.strokeStyle = this.state.options.playercolors[neutral_color_index];
                    this.ctx.beginPath();
                    this.ctx.arc(x1 + w/2 + 0.2 * this.scale, y1 + w, w/3, Math.PI, 2 * Math.PI, true);
                    this.ctx.stroke();
                    
                    // if to determine whether we go clockwise or anti-clockwise
                    this.ctx.beginPath();
                    var angle = (entry[1] - this.time - 1) / 20 * Math.PI;
                    if (owner == null) {
                        this.ctx.strokeStyle = this.state.options.playercolors[entry[0]];
                        this.ctx.arc(x1 + w/2 + 0.2 * this.scale, y1 + w, w/3, Math.PI, angle + Math.PI, true);
                    } else {
                        this.ctx.strokeStyle = this.state.options.playercolors[owner];
                        this.ctx.arc(x1 + w/2 + 0.2 * this.scale, y1 + w, w/3, 2 * Math.PI - angle, Math.PI, false);
                    }
                    this.ctx.stroke();
                    
                    // we found the correct entry for this.time - leave the loop
                    break;
                }
            }
        }
       

        this.ctx.strokeStyle = this.state.options.playercolors[owner];
		this.ctx.fillStyle = this.state.options.playercolors[owner];

        this.ctx.globalAlpha = 1;

        // if the owner is NEUTRAL draw an empty island
		if (owner == null) {
			this.ctx.drawImage(this.islandImage, 0, 0, 256, 256, x1, y1, w, w);
		} else {
			// TODO: use owner to determine index of sprite
			this.ctx.drawImage(this.ownedIslandSprites[this.turn % 3], owner * 256, 0, 256, 256, x1, y1, w, w);

			// TODO: pirate should be in front of ships
			this.ctx.drawImage(this.ownedIslandPirateSprite, owner * 256, 0, 256, 256, x1, y1, w, w);
		}
    }

	// draw pirates sorted by color
	for (hash in this.drawStates) {
		this.ctx.fillStyle = hash;
		drawList = this.drawStates[hash];
		for (n = drawList.length - 1; n >= 0; n--) {
			kf = drawList[n];
			if (kf['owner'] !== undefined) {
				this.drawWrapped(kf.mapX, kf.mapY, this.scale, this.scale, this.w, this.h,
						function(x, y, width) {

							var drawWidth = width * 1.5;
							var drawWidthDouble = drawWidth * 2;

							this.ctx.drawImage(
								this.map.shipSprite,
								kf['owner'] * 128, 0, 128, 128,
								x - drawWidth, y - drawWidth, drawWidthDouble, drawWidthDouble);

						}, [ kf.mapX + halfScale, kf.mapY + halfScale, halfScale * kf['size'] ]);
			} else {
				w = this.scale;
				dx = kf.mapX;
				dy = kf.mapY;
				if (kf['size'] !== 1) {
					d = 0.5 * (1.0 - kf['size']) * this.scale;
					dx += d;
					dy += d;
					w *= kf['size'];
				}
				this.ctx.fillRect(dx, dy, w, w);
			}
		}
	}

	// draw battle indicators
	rows = this.state.replay.rows;
	rowPixels = rows * this.scale;
	cols = this.state.replay.cols;
	colPixels = cols * this.scale;
	this.ctx.lineWidth = Math.pow(this.scale, 0.3);
	for (hash in this.drawStates) {
		drawList = this.drawStates[hash];
		this.ctx.strokeStyle = hash;
		this.ctx.beginPath();
		for (n = drawList.length - 1; n >= 0; n--) {
			kf = drawList[n];
			if (this.pairing[kf.pirateId] !== undefined) {
				for (d = this.pairing[kf.pirateId].targets.length - 1; d >= 0; d--) {
					target = this.pairing[kf.pirateId].targets[d];
					x1 = kf.mapX + halfScale;
					y1 = kf.mapY + halfScale;
					dx = Math.wrapAround(target.mapX - kf.mapX, colPixels);
					if (2 * dx > colPixels) dx -= colPixels;
					x2 = x1 + 0.5 * dx;
					dy = Math.wrapAround(target.mapY - kf.mapY, rowPixels);
					if (2 * dy > rowPixels) dy -= rowPixels;
					y2 = y1 + 0.5 * dy;
					this.drawWrapped(Math.min(x1, x2) - 1, Math.min(y1, y2) - 1,
							Math.abs(x2 - x1) + 2, Math.abs(y2 - y1) + 2, colPixels, rowPixels,
							function(fx1, fy1, fx2, fy2) {
								this.ctx.moveTo(fx1, fy1);
								this.ctx.lineTo(fx2, fy2);
							}, [ x1, y1, x2, y2 ]);
				}
			}
		}
		this.ctx.stroke();
	}

	// draw attack and spawn radiuses
	if (this.mouseOverVis) {
		ar = this.state.replay.meta['replaydata']['attackradius2'];
		ar = this.scale * Math.sqrt(ar);
		sr = this.state.replay.meta['replaydata']['spawnradius2'];
		sr = this.scale * Math.sqrt(sr);
		for (n = this.circledPirates.length - 1; n >= 0; --n) {
			kf = this.circledPirates[n];
			hash = '#';
			hash += INT_TO_HEX[kf['r']];
			hash += INT_TO_HEX[kf['g']];
			hash += INT_TO_HEX[kf['b']];
			this.ctx.strokeStyle = hash;
			this.ctx.beginPath();
			dx = kf.mapX + halfScale;
			dy = kf.mapY + halfScale;
			r = (kf['owner'] === undefined) ? sr : ar;
			x1 = dx - r;
			y1 = dy - r;
			this.ctx.moveTo(dx + r, dy);
			this.ctx.arc(dx, dy, r, 0, 2 * Math.PI, false);
			this.ctx.stroke();
		}
	}

	// draw A, B, C, D ... on pirates or alternatively the global kf id
	label = this.state.config['label'];
	if (label) {
		fontSize = Math.ceil(Math.max(this.scale, 10) / label);
		this.ctx.save();
		this.ctx.translate(halfScale, halfScale);
		this.ctx.textBaseline = 'middle';
		this.ctx.textAlign = 'center';
		this.ctx.font = 'bold ' + fontSize + 'px Arial';
		this.ctx.fillStyle = '#000';
		this.ctx.strokeStyle = '#fff';
		this.ctx.lineWidth = 0.2 * fontSize;
		order = new Array(this.state.order.length);
		for (n = 0; n < order.length; n++) {
			order[this.state.order[n]] = n;
		}
		for (hash in this.drawStates) {
			drawList = this.drawStates[hash];
			for (n = drawList.length - 1; n >= 0; n--) {
				kf = drawList[n];
				if (label === 1) {
					if (kf['owner'] === undefined) continue;
					caption = String.fromCharCode(0x3b1 + order[kf['owner']]);
				} else {
					caption = kf.pirateId;
				}
				this.ctx.strokeText(caption, kf.mapX, kf.mapY);
				this.ctx.fillText(caption, kf.mapX, kf.mapY);
				if (kf.mapX < 0) {
					this.ctx.strokeText(caption, kf.mapX + this.map.w, kf.mapY);
					this.ctx.fillText(caption, kf.mapX + this.map.w, kf.mapY);
					if (kf.mapY < 0) {
						this.ctx.strokeText(caption, kf.mapX + this.map.w, kf.mapY + this.map.h);
						this.ctx.fillText(caption, kf.mapX + this.map.w, kf.mapY + this.map.h);
					}
				}
				if (kf.mapY < 0) {
					this.ctx.strokeText(caption, kf.mapX, kf.mapY + this.map.h);
					this.ctx.fillText(caption, kf.mapX, kf.mapY + this.map.h);
				}
			}
		}
		this.ctx.restore();
	}

	// fog
	if (this.state.fogPlayer !== undefined) {
		dx = (this.fog.w < colPixels) ? ((colPixels - this.fog.w + 1) >> 1) - this.fog.shiftX : 0;
		dy = (this.fog.h < rowPixels) ? ((rowPixels - this.fog.h + 1) >> 1) - this.fog.shiftY : 0;
		this.drawWrapped(dx, dy, this.fog.w, this.fog.h, this.w, this.h, function(ctx, img, x, y) {
			ctx.drawImage(img, x, y);
		}, [ this.ctx, this.fog.canvas, dx, dy ]);
	}
};

/**
* Sets the pirate island image to use when drawing the map.
 *
 * @param {HTMLCanvasElement}
 *        @islandImage a colorized island graphic.
 *        @ownedIslandSprites an array of player-colored island sprites
 */
CanvasElementPiratesMap.prototype.setIslandImage = function(islandImage, ownedIslandSprites, ownedIslandPirateSprite) {
	this.islandImage = islandImage;
	this.ownedIslandSprites = ownedIslandSprites;
	this.ownedIslandPirateSprite = ownedIslandPirateSprite;
};
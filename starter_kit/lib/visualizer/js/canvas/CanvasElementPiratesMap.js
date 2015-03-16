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

// set this to determine the offset of the directional-ship images
var orientation={'e':0, 'n':1, 'w':2, 's':3};

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

CanvasElementPiratesMap.prototype.drawLighthouses = function() {
	var lighthouses = this.state.replay.meta['replaydata']['lighthouses'];
	if (lighthouses) {
		for (var i = 0; i < lighthouses.length; i++) {
			var lighthouse = lighthouses[i];
			var x1 = (lighthouse[1] - 0.75 ) * this.scale;
			var y1 = (lighthouse[0] - 1.5) * this.scale;
			var w = 2.5 * this.scale;


			this.ctx.drawImage(this.map.lighthouse, x1, y1, w, w);
		}
	}
};

CanvasElementPiratesMap.prototype.drawInScale = function(image, sX, sY, sWidth, sHeight, x, y, width, height, scaleSize) {
	var halfScale = 0.5 * this.scale;
	width = width * scaleSize;
	height = height * scaleSize;
	x = x + halfScale - (width / 2);
	y = y + halfScale - (height / 2);
	this.ctx.drawImage(image, sX, sY, sWidth, sHeight, x, y, width, height);
};

CanvasElementPiratesMap.prototype.drawPirates = function() {
	var attackRadius = this.state.replay.meta['replaydata']['attackradius2'];
	attackRadius = this.scale * Math.sqrt(attackRadius);
	for (var hash in this.drawStates) {
		this.ctx.fillStyle = hash;
		var drawList = this.drawStates[hash];
        drawList.sort(function(a, b) {return a.mapY < b.mapY});
		for (n = drawList.length - 1; n >= 0; n--) {
			var kf = drawList[n];
			if (kf['owner'] !== undefined) {
				this.ctx.globalAlpha = kf['cloaked'];
				this.drawWrapped(kf.mapX, kf.mapY, this.scale, this.scale, this.w, this.h,
					function (x, y, width) {
						this.drawInScale(
							this.map.shipSprite,
							kf['owner'] * 200, orientation[kf['orientation']] * 200, 200, 200,
							x, y, width, width, 2);
					}, [kf.mapX, kf.mapY, this.scale * kf['size']]);

				if (this.state.config['showRange'] && kf['cloaked'] === 1) {
					this.ctx.globalAlpha = 0.1;
					this.ctx.beginPath();
					this.ctx.strokeStyle = this.state.options.playercolors[kf['owner']];
					this.ctx.arc(kf.mapX + this.scale / 2, kf.mapY + this.scale / 2, attackRadius, 0, 2 * Math.PI, false);
					this.ctx.fill()
				}
			} else {
				var w = this.scale;
				var dx = kf.mapX;
				var dy = kf.mapY;
				if (kf['size'] !== 1) {
					var d = 0.5 * (1.0 - kf['size']) * this.scale;
					dx += d;
					dy += d;
					w *= kf['size'];
				}
				this.ctx.fillRect(dx, dy, w, w);
			}
		}
	}
	this.ctx.globalAlpha = 1;
};

CanvasElementPiratesMap.prototype.drawBlockedMoves = function() {
	var rejected = this.state.replay.meta['replaydata']['rejected'];
	for (var i = 0; i < rejected.length; i++) {
		var rej = rejected[i];
		if (rej[0] > this.turn + 1) {
			// no need to check this anymore - only relevant in future
			break;
		}
		if ((rej[0] > this.time) && (rej[0] < (this.time + 1))) {
			var centerx = (rej[2] + 0.5) * this.scale;
			var centery = (rej[1] + 0.5) * this.scale;
			var dir = undefined;
			var interpol = 1 - rej[0] + this.time;
			dir = Direction.fromChar(rej[3]);
			this.ctx.lineWidth = 4;
			this.ctx.strokeStyle = "#FF0000";
			this.ctx.beginPath();
			// make this line a little bigger
			this.ctx.arc(centerx, centery, this.scale * 0.5,
				dir.angle - Math.PI / 2 + Math.PI / 4,
				dir.angle - Math.PI / 2 - Math.PI / 4, true);
			this.ctx.stroke();
		}
	}
};

CanvasElementPiratesMap.prototype.drawBattleIndicators = function() {
	var halfScale = 0.5 * this.scale;
	var rows = this.state.replay.rows;
	var rowPixels = rows * this.scale;
	var cols = this.state.replay.cols;
	var colPixels = cols * this.scale;
	this.ctx.lineWidth = Math.pow(this.scale, 0.3);
	for (var hash in this.drawStates) {
		var drawList = this.drawStates[hash];
		this.ctx.strokeStyle = hash;
		this.ctx.beginPath();
		for (var n = drawList.length - 1; n >= 0; n--) {
			var kf = drawList[n];
			if (this.pairing[kf.pirateId] !== undefined) {
				for (d = this.pairing[kf.pirateId].targets.length - 1; d >= 0; d--) {
					var target = this.pairing[kf.pirateId].targets[d];
					var x1 = kf.mapX + halfScale;
					var y1 = kf.mapY + halfScale;
					var dx = Math.wrapAround(target.mapX - kf.mapX, colPixels);
					if (2 * dx > colPixels) dx -= colPixels;
					var x2 = x1 + 0.5 * dx;
					var dy = Math.wrapAround(target.mapY - kf.mapY, rowPixels);
					if (2 * dy > rowPixels) dy -= rowPixels;
					var y2 = y1 + 0.5 * dy;
					this.drawWrapped(Math.min(x1, x2) - 1, Math.min(y1, y2) - 1,
						Math.abs(x2 - x1) + 2, Math.abs(y2 - y1) + 2, colPixels, rowPixels,
						function (fx1, fy1, fx2, fy2) {
							this.ctx.moveTo(fx1, fy1);
							this.ctx.lineTo(fx2, fy2);
						}, [x1, y1, x2, y2]);
				}
			}
		}
		this.ctx.stroke();
	}
};

CanvasElementPiratesMap.prototype.drawAttackRadiuses = function() {
	var halfScale = 0.5 * this.scale;
	if (this.mouseOverVis) {
		var ar = this.state.replay.meta['replaydata']['attackradius2'];
		ar = this.scale * Math.sqrt(ar);
		for (var n = this.circledPirates.length - 1; n >= 0; --n) {
			var kf = this.circledPirates[n];
			var hash = '#';
			hash += INT_TO_HEX[kf['r']];
			hash += INT_TO_HEX[kf['g']];
			hash += INT_TO_HEX[kf['b']];
			this.ctx.strokeStyle = hash;
			this.ctx.beginPath();
			var dx = kf.mapX + halfScale;
			var dy = kf.mapY + halfScale;
			var x1 = dx - ar;
			var y1 = dy - ar;
			this.ctx.moveTo(dx + ar, dy);
			this.ctx.arc(dx, dy, ar, 0, 2 * Math.PI, false);
			this.ctx.stroke();
		}
	}
};

CanvasElementPiratesMap.prototype.drawLabels = function() {
	var label = this.state.config['label'];
	if (label) {
		var fontSize = Math.ceil(Math.max(this.scale, 10) / label);
		this.ctx.save();
		this.ctx.translate(halfScale, halfScale);
		this.ctx.textBaseline = 'middle';
		this.ctx.textAlign = 'center';
		this.ctx.font = 'bold ' + fontSize + 'px Arial';
		this.ctx.fillStyle = '#000';
		this.ctx.strokeStyle = '#fff';
		this.ctx.lineWidth = 0.2 * fontSize;
		var order = new Array(this.state.order.length);
		for (n = 0; n < order.length; n++) {
			order[this.state.order[n]] = n;
		}
		for (var hash in this.drawStates) {
			var drawList = this.drawStates[hash];
			for (var n = drawList.length - 1; n >= 0; n--) {
				var kf = drawList[n];
				if (label === 1) {
					if (kf['owner'] === undefined) continue;
					var caption = String.fromCharCode(0x3b1 + order[kf['owner']]);
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
};

CanvasElementPiratesMap.prototype.drawFog = function() {
	var rows = this.state.replay.rows;
	var rowPixels = rows * this.scale;
	var cols = this.state.replay.cols;
	var colPixels = cols * this.scale;
	if (this.state.fogPlayer !== undefined) {
		var dx = (this.fog.w < colPixels) ? ((colPixels - this.fog.w + 1) >> 1) - this.fog.shiftX : 0;
		var dy = (this.fog.h < rowPixels) ? ((rowPixels - this.fog.h + 1) >> 1) - this.fog.shiftY : 0;
		this.drawWrapped(dx, dy, this.fog.w, this.fog.h, this.w, this.h, function (ctx, img, x, y) {
			ctx.drawImage(img, x, y);
		}, [this.ctx, this.fog.canvas, dx, dy]);
	}
};

CanvasElementPiratesMap.prototype.drawCapturingBar = function(island, x, y, w, owner) {
	// Generate a progress-bar over island being captured
	// this.ctx.globalAlpha is always 1. use it to fade in.
	// this if causes code to be skipped in games that dont have this data structure
	this.ctx.lineWidth = 0.5 * this.scale;
	var neutral_color_index = this.state.options.playercolors.length - 1;
	var self = this;

	function drawArc(angle, clockwise) {
		if (clockwise) {
			self.ctx.arc(x + self.scale, y + self.scale*1.5, w / 2.3, Math.PI, angle + Math.PI, clockwise);
		} else {
			self.ctx.arc(x + self.scale, y + self.scale*1.5, w / 2.3, 2 * Math.PI - angle, Math.PI, clockwise);
		}
	}

	if (island.length > 3) {
		var attack_history = island[3];
        var capture_turns = island[4];
        if (capture_turns == undefined) {
            capture_turns = this.state.replay.meta['replaydata']['captureturns'];
        }
		var entry = null;
		for (var j = 0; j < attack_history.length; j++) {
			entry = attack_history[j];
			if ((this.turn >= entry[1]) && (this.turn < entry[1] + entry[2] - 1)) {
				// Background always NEUTRAL
				this.ctx.strokeStyle = this.state.options.playercolors[neutral_color_index];
				this.ctx.beginPath();
				drawArc(Math.PI, true);
				this.ctx.stroke();

				// if to determine whether we go clockwise or anti-clockwise
				this.ctx.beginPath();
				var angle = (entry[1] - this.time - 1) / capture_turns * Math.PI;
				if (owner == null) {
					this.ctx.strokeStyle = this.state.options.playercolors[entry[0]];
					drawArc(angle, true);
				} else {
					this.ctx.strokeStyle = this.state.options.playercolors[owner];
					drawArc(angle, false);
				}
				this.ctx.stroke();

				// we found the correct entry for this.time - leave the loop
				break;
			}
		}
	}
};

CanvasElementPiratesMap.prototype.drawIslands = function() {
	var islands = this.state.replay.meta['replaydata']['forts'];
	for (var i = 0; i < islands.length; i++) {
		var island = islands[i];
		var x = (island[1] - 0.5) * this.scale;
		var y = (island[0] - 0.5) * this.scale;
		// or 0 just in case field doesnt exist
		var island_value_offset = (island[5] - 1) * 200 || 0;
		var w = this.scale;
		var size = 5;

		// find the current owner. iterate through states until you get to a future state, then just take the previous most one.
		var owners = island[2];
		var owner = owners[0][1];
		for (var j = 1; j < owners.length; j++) {
			var status = owners[j];
			if (this.turn >= status[0]) {
				owner = status[1];
			} else {
				break;
			}
		}

		this.drawCapturingBar(island, x, y, w * size, owner);

		this.ctx.strokeStyle = this.state.options.playercolors[owner];
		this.ctx.fillStyle = this.state.options.playercolors[owner];

		this.ctx.globalAlpha = 1;
		// if the owner is NEUTRAL draw an empty island
		if (owner == null) {
			this.drawInScale(this.islandImage, 0, island_value_offset, 200, 200, x, y, w, w, size);
		} else {
			this.drawInScale(this.ownedIslandSeaSprite, owner * 200, 0, 200, 200, x, y, w, w, size);
			this.drawInScale(this.ownedIslandSprites[this.turn % 3], owner * 200, island_value_offset, 200, 200, x, y, w, w, size);
		}
	}
};

/**
 * Draws pirates onto the map image. This includes overlay letters / ids, attack lines, effect circles
 * and finally the fog of war.
 */
CanvasElementPiratesMap.prototype.draw = function () {
	this.ctx.drawImage(this.map.canvas, 0, 0);

	this.drawIslands();
	this.drawLighthouses();
	this.drawPirates();
	this.drawBlockedMoves();
	this.drawBattleIndicators();
	this.drawAttackRadiuses();
	this.drawLabels();
	this.drawFog();
};

/**
* Sets the pirate island image to use when drawing the map.
 *
 * @param {HTMLCanvasElement}
 *        @islandImage a colorized island graphic.
 *        @ownedIslandSprites an array of player-colored island sprites
 */
CanvasElementPiratesMap.prototype.setIslandImage = function(islandImage, ownedIslandSprites, ownedIslandSeaSprite) {
	this.islandImage = islandImage;
	this.ownedIslandSprites = ownedIslandSprites;
	this.ownedIslandSeaSprite = ownedIslandSeaSprite;
};
(function($, undefined) {
	$.cmenu = function(options, element, callback) {
		this.$el = $(element);
		this.initialize(options);
		this.$callback = callback;
	};
	$.cmenu.defaults = {
		current : 0, // index of current item
		interval : 30
	// time between transitions
	};
	$.cmenu.$left3DCss = {
		'-webkit-transform' : 'translateX(-50%) translateZ(-200px) rotateY(45deg)',
		'-moz-transform' : 'translateX(-50%) translateZ(-200px) rotateY(45deg)',
		'-o-transform' : 'translateX(-50%) translateZ(-200px) rotateY(45deg)',
		'-ms-transform' : 'translateX(-50%) translateZ(-200px) rotateY(45deg)',
		'transform' : 'translateX(-50%) translateZ(-200px) rotateY(45deg)',
		'opacity' : 1,
		'visibility' : 'visible',
		'z-index' : 2
	};
	$.cmenu.$right3DCss = {
		'-webkit-transform' : 'translateX(50%) translateZ(-200px) rotateY(-45deg)',
		'-moz-transform' : 'translateX(50%) translateZ(-200px) rotateY(-45deg)',
		'-o-transform' : 'translateX(50%) translateZ(-200px) rotateY(-45deg)',
		'-ms-transform' : 'translateX(50%) translateZ(-200px) rotateY(-45deg)',
		'transform' : 'translateX(50%) translateZ(-200px) rotateY(-45deg)',
		'opacity' : 1,
		'visibility' : 'visible',
		'z-index' : 2
	};
	$.cmenu.$center3DCss = {
		'-webkit-transform' : 'translateX(0px) translateZ(0px) rotateY(0deg)',
		'-moz-transform' : 'translateX(0px) translateZ(0px) rotateY(0deg)',
		'-o-transform' : 'translateX(0px) translateZ(0px) rotateY(0deg)',
		'-ms-transform' : 'translateX(0px) translateZ(0px) rotateY(0deg)',
		'transform' : 'translateX(0px) translateZ(0px) rotateY(0deg)',
		'opacity' : 1,
		'visibility' : 'visible',
		'z-index' : 3
	};
	$.cmenu.$outRight3DCss = {
		'-webkit-transform' : 'translateX(50%) translateZ(-300px) rotateY(-45deg)',
		'-moz-transform' : 'translateX(50%) translateZ(-300px) rotateY(-45deg)',
		'-o-transform' : 'translateX(50%) translateZ(-300px) rotateY(-45deg)',
		'-ms-transform' : 'translateX(50%) translateZ(-300px) rotateY(-45deg)',
		'transform' : 'translateX(50%) translateZ(-300px) rotateY(-45deg)',
		'opacity' : 0,
		'visibility' : 'hidden',
		'z-index' : 2
	};
	$.cmenu.$outLeft3DCss = {
		'-webkit-transform' : 'translateX(-50%) translateZ(-300px) rotateY(45deg)',
		'-moz-transform' : 'translateX(-50%) translateZ(-300px) rotateY(45deg)',
		'-o-transform' : 'translateX(-50%) translateZ(-300px) rotateY(45deg)',
		'-ms-transform' : 'translateX(-50%) translateZ(-300px) rotateY(45deg)',
		'transform' : 'translateX(-50%) translateZ(-300px) rotateY(45deg)',
		'opacity' : 0,
		'visibility' : 'hidden',
		'z-index' : 2
	};
	$.cmenu.$left2DCss = {
		'-webkit-transform' : 'translate(-25%) scale(0.8)',
		'-moz-transform' : 'translate(-25%) scale(0.8)',
		'-o-transform' : 'translate(-25%) scale(0.8)',
		'-ms-transform' : 'translate(-25%) scale(0.8)',
		'transform' : 'translate(-25%) scale(0.8)',
		'opacity' : 1,
		'visibility' : 'visible',
		'z-index' : 2
	};
	$.cmenu.$right2DCss = {
		'-webkit-transform' : 'translate(25%) scale(0.8)',
		'-moz-transform' : 'translate(25%) scale(0.8)',
		'-o-transform' : 'translate(25%) scale(0.8)',
		'-ms-transform' : 'translate(25%) scale(0.8)',
		'transform' : 'translate(25%) scale(0.8)',
		'opacity' : 1,
		'visibility' : 'visible',
		'z-index' : 2
	};
	$.cmenu.$center3DCss = {
		'-webkit-transform' : 'translate(0px) scale(1)',
		'-moz-transform' : 'translate(0px) scale(1)',
		'-o-transform' : 'translate(0px) scale(1)',
		'-ms-transform' : 'translate(0px) scale(1)',
		'transform' : 'translate(0px) scale(1)',
		'opacity' : 1,
		'visibility' : 'visible',
		'z-index' : 3
	};
	$.cmenu.$outRight3DCss = {
		'-webkit-transform' : 'translate(450px) scale(0.7)',
		'-moz-transform' : 'translate(450px) scale(0.7)',
		'-o-transform' : 'translate(450px) scale(0.7)',
		'-ms-transform' : 'translate(450px) scale(0.7)',
		'transform' : 'translate(450px) scale(0.7)',
		'opacity' : 0,
		'visibility' : 'hidden',
		'z-index' : 2
	};
	$.cmenu.$outLeft3DCss = {
		'-webkit-transform' : 'translate(-450px) scale(0.7)',
		'-moz-transform' : 'translate(-450px) scale(0.7)',
		'-o-transform' : 'translate(-450px) scale(0.7)',
		'-ms-transform' : 'translate(-450px) scale(0.7)',
		'transform' : 'translate(-450px) scale(0.7)',
		'opacity' : 0,
		'visibility' : 'hidden',
		'z-index' : 2
	};
	$.cmenu.prototype = {
		initialize : function(options) {
			this.options = $.extend(true, {}, $.cmenu.defaults, options);
			// support for 3d / 2d transforms and transitions
			this.support3d = Modernizr.csstransforms3d;
			this.support2d = Modernizr.csstransforms;
			this.supportTrans = Modernizr.csstransitions;
			this.$wrapper = this.$el.find('.cmenu-wrapper');
			this.$items = this.$wrapper.children();
			this.itemsCount = this.$items.length;
			this.$nav = this.$el.find('nav');
			if (this.itemsCount < 3) {
				this.$nav.remove();
				return false;
			}
			this.current = this.options.current;
			this.isAnim = false;
			this.$items.css({
				'opacity' : 0,
				'visibility' : 'hidden'
			});
			this.validate();
			this.loadEvents();
			this.layout();
		},
		validate : function() {
			if (this.options.current < 0
					|| this.options.current > this.itemsCount - 1) {
				this.current = 0;
			}
		},
		layout : function() {
			this.setItems();
			var leftCSS, rightCSS, currentCSS;
			if (this.support3d && this.supportTrans) {
				leftCSS = $.cmenu.$left3DCss;
				rightCSS = $.cmenu.$right3DCss;
				currentCSS = $.cmenu.$center3DCss;
			} else if (this.support2d && this.supportTrans) {
				leftCSS = $.cmenu.$left2DCss;
				rightCSS = $.cmenu.$left2DCss
				currentCSS = $.cmenu.$center2DCss;
			}
			this.$leftItm.css(leftCSS || {});
			this.$rightItm.css(rightCSS || {});
			this.$currentItm.css(currentCSS || {}).addClass('ActiveTab');
		},
		setItems : function() {
			this.$items.removeClass('ActiveTab');
			this.$currentItm = this.$items.eq(this.current);
			this.$leftItm = (this.current === 0) ? this.$items
					.eq(this.itemsCount - 1) : this.$items.eq(this.current - 1);
			this.$rightItm = (this.current === this.itemsCount - 1) ? this.$items
					.eq(0)
					: this.$items.eq(this.current + 1);
			if (this.itemsCount > 3) {
				// next item
				this.$nextItm = (this.$rightItm.index() === this.itemsCount - 1) ? this.$items
						.eq(0)
						: this.$rightItm.next();
				this.$nextItm.css(this.getCss('outright'));
				// previous item
				this.$prevItm = (this.$leftItm.index() === 0) ? this.$items
						.eq(this.itemsCount - 1) : this.$leftItm.prev();
				this.$prevItm.css(this.getCss('outleft'));
			}
		},
		loadEvents : function() {
			var self = this;
			this.$wrapper
					.on(
							'webkitTransitionEnd.cmenu transitionend.cmenu OTransitionEnd.cmenu',
							function(event) {
								self.$currentItm.addClass('ActiveTab');
								self.$items.removeClass('cmenu-transition');
								self.isAnim = false;
							});
		},
		getCss : function(position) {
			if (this.support3d && this.supportTrans) {
				switch (position) {
				case 'outleft':
					return $.cmenu.$outLeft3DCss;
					break;
				case 'outright':
					return $.cmenu.$outRight3DCss;
					break;
				case 'left':
					return $.cmenu.$left3DCss;
					break;
				case 'right':
					return $.cmenu.$right3DCss;
					break;
				case 'center':
					return $.cmenu.$center3DCss;
					break;
				}
				;
			} else if (this.support2d && this.supportTrans) {
				switch (position) {
				case 'outleft':
					return $.cmenu.$outLeft2DCss;
					break;
				case 'outright':
					return $.cmenu.$outRight2DCss;
					break;
				case 'left':
					return $.cmenu.$left2DCss;
					break;
				case 'right':
					return $.cmenu.$right2DCss;
					break;
				case 'center':
					return $.cmenu.$center2DCss;
					break;
				}
				;
			} else {
				switch (position) {
				case 'outleft':
				case 'outright':
				case 'left':
				case 'right':
					return {
						'opacity' : 0,
						'visibility' : 'hidden'
					};
					break;
				case 'center':
					return {
						'opacity' : 1,
						'visibility' : 'visible'
					};
					break;
				}
				;
			}
		},
		navigate : function(dir) {
			if (this.supportTrans && this.isAnim)
				return false;
			this.isAnim = true;
			switch (dir) {
			case 'next':
				this.current = this.$rightItm.index();
				this.$currentItm.css(this.getCss('left')).addClass(
						'cmenu-transition');
				this.$rightItm.css(this.getCss('center')).addClass(
						'cmenu-transition');
				if (this.$nextItm) {
					this.$leftItm.css(this.getCss('outleft')).addClass(
							'cmenu-transition');
					this.$nextItm.css(this.getCss('right')).addClass(
							'cmenu-transition');
				} else {
					this.$leftItm.css(this.getCss('right')).addClass(
							'cmenu-transition');
				}
				break;
			case 'prev':
				this.current = this.$leftItm.index();
				this.$currentItm.css(this.getCss('right')).addClass(
						'cmenu-transition');
				this.$leftItm.css(this.getCss('center')).addClass(
						'cmenu-transition');
				if (this.$prevItm) {
					this.$rightItm.css(this.getCss('outright')).addClass(
							'cmenu-transition');
					this.$prevItm.css(this.getCss('left')).addClass(
							'cmenu-transition');
				} else {
					this.$rightItm.css(this.getCss('left')).addClass(
							'cmenu-transition');
				}
				break;
			}
			;
			var current = this.$currentItm[0];
			this.setItems();
			this.$currentItm.addClass('ActiveTab');
			this.$callback(this.$currentItm[0], current);
		},
		destroy : function() {
			this.$wrapper.off('.cmenu');
		}
	};
	$.fn.cmenu = function(options, activeCallback) {
		if (typeof options === 'string') {
			var args = Array.prototype.slice.call(arguments, 1);
			this.each(function() {
				var instance = $.data(this, 'cmenu');
				if (!instance) {
					return;
				}
				if (!$.isFunction(instance[options])
						|| options.charAt(0) === "_") {
					return;
				}
				instance[options].apply(instance, args);
			});
		} else {
			this.each(function() {
				var instance = $.data(this, 'cmenu');
				if (!instance) {
					$.data(this, 'cmenu', new $.cmenu(options, this,
							activeCallback));
				}
			});
		}
		return this;
	};
})(jQuery);
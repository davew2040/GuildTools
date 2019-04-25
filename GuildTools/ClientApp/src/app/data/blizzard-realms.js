"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var BlizzardRegionDefinition = /** @class */ (function () {
    function BlizzardRegionDefinition(name, code) {
        this.name = name;
        this.code = code;
    }
    Object.defineProperty(BlizzardRegionDefinition.prototype, "Name", {
        get: function () {
            return this.name;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(BlizzardRegionDefinition.prototype, "Code", {
        get: function () {
            return this.code;
        },
        enumerable: true,
        configurable: true
    });
    return BlizzardRegionDefinition;
}());
exports.BlizzardRegionDefinition = BlizzardRegionDefinition;
var BlizzardRealms = /** @class */ (function () {
    function BlizzardRealms() {
    }
    Object.defineProperty(BlizzardRealms, "AllRealms", {
        get: function () {
            return [
                BlizzardRealms.US,
                BlizzardRealms.EU
            ];
        },
        enumerable: true,
        configurable: true
    });
    BlizzardRealms.US = new BlizzardRegionDefinition("US", 1);
    BlizzardRealms.EU = new BlizzardRegionDefinition("EU", 2);
    return BlizzardRealms;
}());
exports.BlizzardRealms = BlizzardRealms;
//# sourceMappingURL=blizzard-realms.js.map
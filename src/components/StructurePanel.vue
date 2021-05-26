<template>
  <div>
    <b-sidebar
      id="leftsidepanel"
      class="b-sidebar mr-5"
      title="Network information"
      left
      shadow=true
      width="320px"
      v-model="isPanelOpen"
      no-header-close
    >
      <div id="treebox">
        <vue-tree-list
          @click="onClick"

          :model="data"
          default-tree-node-name="new node"
          default-leaf-node-name="new leaf"
          v-bind:default-expanded="false"
        >
        <template v-slot:leafNameDisplay="slotProps">
          <span>
            {{ slotProps.model.name }}
          </span>
        </template>
        <span class="icon" slot="leafNodeIcon">🍃</span>
        <span class="icon" slot="treeNodeIcon">🌲</span>
        </vue-tree-list>
      </div>
    </b-sidebar>
  </div>
</template>

<script lang="ts">
import SidebarPanel from 'bootstrap-vue'
// @ts-ignore
import { VueTreeList, Tree, TreeNode } from 'vue-tree-list'
import axios from "axios"
import Vue from 'vue'
import { Component, Prop } from 'vue-property-decorator'

// @ts-ignore
@Component({
  components: {
    SidebarPanel,
    VueTreeList
  }
})
export default class StructurePanel extends Vue {

  isPanelOpen: boolean = true
  newTree: any = {}
  data: any = Tree

  @Prop()
  treeNodes!: any[]

  async mounted () {
    await this.getData()
  }

  async getData () {
    this.data = new Tree(this.treeNodes)
  }

  onNodeClick () {
    if (!this.$data.isPanelOpen) {
      this.$data.isPanelOpen = true
    }
  }

  onClick(treeNode: TreeNode) {
    if (treeNode.server_id != null){
      this.$emit('structure-node-click', {name: (treeNode.name).toString(), server_id: (treeNode.server_id).toString()})
    }
  }

  addNode() {
    var node = new TreeNode({ name: 'new node', isLeaf: false })
    if (!this.data.children) this.data.children = []
    this.data.addChildren(node)
  }

  getNewTree() {
    var vm = this
    function _dfs(oldNode: TreeNode) {
      var newNode: TreeNode = {}

      for (var k in oldNode) {
        if (k !== 'children' && k !== 'parent') {
          newNode[k] = oldNode[k]
        }
      }

      if (oldNode.children && oldNode.children.length > 0) {
        newNode.children = []
        for (var i = 0, len = oldNode.children.length; i < len; i++) {
          newNode.children.push(_dfs(oldNode.children[i]))
        }
      }
      return newNode
    }

    vm.newTree = _dfs(vm.data)
  }
}
</script>

<style scoped>
#treebox {
  margin-left: 13px;
  margin-right: 13px;
}

</style>

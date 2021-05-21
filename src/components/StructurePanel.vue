<template>
  <div>
    <b-sidebar
      id="leftsidepanel"
      class="b-sidebar mr-5"
      title="Network information"
      left
      shadow=true
      width="300px"
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
import { VueTreeList, Tree, TreeNode } from 'vue-tree-list'
import axios from "axios";

export default {
  name: "StructurePanel",
  components: { SidebarPanel, VueTreeList },
  async mounted () {
    await this.getData()
  },
  data() {
    return {
      isPanelOpen: true,
      newTree: {},
      data: Tree
    }
  },
  methods: {
    async getData () {
      const TreeViewData = await axios.get('https://localhost:5001/TreeViewData')
      this.data = new Tree(TreeViewData.data)
      console.log("TEST TEST TEST")
      console.log(this.data)
      console.log("TEST TEST TEST")
    },
    onNodeClick () {
      if (!this.$data.isPanelOpen) {
        this.$data.isPanelOpen = true
      }
    },
    onClick(params) {
      console.log(params.server_id)
      console.log(params.name)
      if (params.server_id != null){
        this.$emit('structure-node-click', {name: (params.name).toString(), server_id: (params.server_id).toString()})
      }
    },
    addNode() {
      var node = new TreeNode({ name: 'new node', isLeaf: false })
      if (!this.data.children) this.data.children = []
      this.data.addChildren(node)
    },
    getNewTree() {
      var vm = this
      function _dfs(oldNode) {
        var newNode = {}

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
}
</script>

<style scoped>
#treebox {
  margin-left: 13px;
  margin-right: 13px;
}

/*.b-sidebar{
  /*position: absolute;
  padding: 10px;
  margin-right: 200px;
  background-color: blueviolet;
}
.b-avatar{
  position: absolute;
}*/

</style>

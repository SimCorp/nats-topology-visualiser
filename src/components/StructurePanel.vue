<template>
  <div>
    <b-sidebar
      id="leftsidepanel"
      class="b-sidebar mr-5"
      margin-left="100px"
      title="Structure Panel"
      left
      shadow=true
      width="400px"
      v-model="isPanelOpen">
      <div>
    <vue-tree-list
      @click="onClick"

      :model="data"
      default-tree-node-name="new node"
      default-leaf-node-name="new leaf"
      v-bind:default-expanded="false"
    >
      <template v-slot:leafNameDisplay="slotProps">
        <span>
          {{ slotProps.model.name }} <span class="muted">#{{ slotProps.model.id }}</span>
        </span>
      </template>
      <span class="icon" slot="leafNodeIcon">🍃</span>
      <span class="icon" slot="treeNodeIcon">🌲</span>
    </vue-tree-list>
  </div>
    <p>{{this.data}}</p>
    </b-sidebar>
  </div>
</template>

<script lang="ts">
import SidebarPanel from 'bootstrap-vue'
import { VueTreeList, Tree, TreeNode } from 'vue-tree-list'
import axios from "axios";

export default {
  name: "StructurePanel",
  async mounted () {
    await this.getData()
  },
  components: { SidebarPanel, VueTreeList },

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
        console.log(params)
        //this.$emit('structure-node-click', {nodeData: a.data, id: params.}) //TODO FIX THIS
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

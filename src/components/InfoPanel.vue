<template>
  <div>
    <b-sidebar
      id="sidepanel"
      title="Node information"
      right
      backdrop-variant="transparent"
      backdrop
      shadow=true
      width="400px"
      v-model="isPanelOpen">
      <div class="px-3 py-2">
        <json-view :data="nodeData" :root-key="rootName"/>
      </div>
    </b-sidebar>
  </div>
</template>

<script lang="ts">
import SidebarPanel from 'bootstrap-vue'

export default {
  name: "InfoPanel",
  components: { SidebarPanel },
  data () {
    return {
      isPanelOpen: false,
      nodeData: null,
      rootName: 'node'
    }
  },
  methods: {
    onNodeClick (nodeData) {
      if (!this.$data.isPanelOpen) {
        this.$data.isPanelOpen = true
      }
      this.nodeData = nodeData
    }
  }
}
</script>

<style scoped>

</style>
